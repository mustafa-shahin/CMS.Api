using CMS.Application.Common.Extensions;
using CMS.Application.Common.Models.Search;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace CMS.Application.Common.Services;

/// <summary>
/// Generic search service with PostgreSQL full-text search, filtering, sorting, and paging
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Execute search query with PostgreSQL full-text search and relevance scoring
    /// </summary>
    Task<SearchResult<T>> SearchAsync<T>(
        IQueryable<T> baseQuery,
        SearchRequest request,
        SearchConfiguration<T> configuration,
        CancellationToken cancellationToken = default) where T : class;
}

/// <summary>
/// Implementation of search service with PostgreSQL full-text search
/// </summary>
public sealed class SearchService : ISearchService
{
    private const string DefaultLanguage = "english";

    public async Task<SearchResult<T>> SearchAsync<T>(
        IQueryable<T> baseQuery,
        SearchRequest request,
        SearchConfiguration<T> configuration,
        CancellationToken cancellationToken = default) where T : class
    {
        var stopwatch = Stopwatch.StartNew();

        // Validate and sanitize request
        var validatedRequest = ValidateRequest(request, configuration);

        // Start with base query
        var query = baseQuery;

        // Apply filters first
        var filterExpression = DynamicQueryBuilder.BuildFilterExpression<T>(
            validatedRequest.Filters,
            configuration.FilterableFields.Keys.ToHashSet());

        if (filterExpression != null)
        {
            query = query.Where(filterExpression);
        }

        // Track if we need to use full-text search
        var useFullTextSearch = !string.IsNullOrWhiteSpace(validatedRequest.SearchTerm) &&
                                 validatedRequest.SearchTerm.Length >= configuration.MinSearchTermLength &&
                                 configuration.SearchableFields.Count > 0;

        IQueryable<SearchItemWithScore<T>>? scoredQuery = null;

        if (useFullTextSearch)
        {
            // Apply PostgreSQL full-text search with relevance scoring
            scoredQuery = ApplyFullTextSearchWithScoring(query, validatedRequest, configuration);

            // Filter by minimum relevance score if specified
            if (validatedRequest.MinRelevanceScore.HasValue)
            {
                scoredQuery = scoredQuery.Where(x => x.Score >= validatedRequest.MinRelevanceScore.Value);
            }

            // Get total count with FTS applied
            var totalCountWithSearch = await scoredQuery.CountAsync(cancellationToken);

            // Apply sorting (by relevance score descending by default, then custom sorts)
            scoredQuery = ApplySortingToScoredQuery(scoredQuery, validatedRequest.Sorts, configuration);

            // Apply pagination
            var skip = (validatedRequest.PageNumber - 1) * validatedRequest.PageSize;
            scoredQuery = scoredQuery.Skip(skip).Take(validatedRequest.PageSize);

            // Execute query and project to result
            var scoredItems = await scoredQuery.ToListAsync(cancellationToken);

            stopwatch.Stop();

            return new SearchResult<T>
            {
                Items = scoredItems.Select(item => new SearchResultItem<T>
                {
                    Data = item.Entity,
                    RelevanceScore = item.Score
                }).ToList(),
                TotalCount = totalCountWithSearch,
                PageNumber = validatedRequest.PageNumber,
                PageSize = validatedRequest.PageSize,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                AppliedFilters = validatedRequest.Filters,
                AppliedSorts = validatedRequest.Sorts,
                SearchTerm = validatedRequest.SearchTerm
            };
        }
        else
        {
            // No full-text search, just filtering and sorting
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = DynamicQueryBuilder.ApplySorting(
                query,
                validatedRequest.Sorts,
                configuration.SortableFields,
                configuration.DefaultSortField,
                configuration.DefaultSortDescending);

            // Apply pagination
            var skip = (validatedRequest.PageNumber - 1) * validatedRequest.PageSize;
            query = query.Skip(skip).Take(validatedRequest.PageSize);

            // Execute query
            var items = await query.ToListAsync(cancellationToken);

            stopwatch.Stop();

            return new SearchResult<T>
            {
                Items = items.Select(item => new SearchResultItem<T>
                {
                    Data = item,
                    RelevanceScore = null
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = validatedRequest.PageNumber,
                PageSize = validatedRequest.PageSize,
                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
                AppliedFilters = validatedRequest.Filters,
                AppliedSorts = validatedRequest.Sorts,
                SearchTerm = validatedRequest.SearchTerm
            };
        }
    }

    private SearchRequest ValidateRequest<T>(SearchRequest request, SearchConfiguration<T> configuration) where T : class
    {
        // Sanitize search term
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            request.SearchTerm = request.SearchTerm.SanitizeSearchQuery();
        }

        // Validate page number
        if (request.PageNumber < 1)
        {
            request.PageNumber = 1;
        }

        // Validate and cap page size
        if (request.PageSize < 1)
        {
            request.PageSize = configuration.DefaultPageSize;
        }

        if (request.PageSize > configuration.MaxPageSize)
        {
            request.PageSize = configuration.MaxPageSize;
        }

        return request;
    }

    /// <summary>
    /// Applies PostgreSQL full-text search with relevance scoring using expression trees.
    ///
    /// HOW THIS WORKS - Expression Tree Building with DbFunctions:
    /// ===========================================================
    /// 1. We build LINQ expression trees using Expression.Call to call PostgreSqlFunctions methods
    /// 2. These expression trees are NOT executed in C# - they're analyzed by EF Core
    /// 3. EF Core translates the expression trees to SQL using the DbFunction mappings
    /// 4. The actual search happens in PostgreSQL using native full-text search
    ///
    /// Example of what we build:
    /// C# Expression Tree:
    ///   Expression.Call(PostgreSqlFunctions.ToTsVector, "english", property)
    ///
    /// EF Core translates to SQL:
    ///   to_tsvector('english', "PropertyName")
    ///
    /// The PostgreSqlFunctions methods are NEVER executed - they're just markers
    /// for EF Core to know which SQL functions to generate.
    ///
    /// Final SQL generated (example for Users):
    /// WHERE (
    ///     setweight(to_tsvector('english', coalesce("Email", '')), 'A') ||
    ///     setweight(to_tsvector('english', coalesce("FirstName", '')), 'B') ||
    ///     setweight(to_tsvector('english', coalesce("LastName", '')), 'B')
    /// ) @@ websearch_to_tsquery('english', 'search term')
    /// </summary>
    private IQueryable<SearchItemWithScore<T>> ApplyFullTextSearchWithScoring<T>(
        IQueryable<T> query,
        SearchRequest request,
        SearchConfiguration<T> configuration) where T : class
    {
        var sanitizedQuery = request.SearchTerm!.SanitizeSearchQuery();
        var language = configuration.Language;

        // Build the search vector expression dynamically based on searchable fields
        // This creates an expression tree that EF Core will translate to SQL
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? searchVectorExpression = null;

        foreach (var (fieldName, weight) in configuration.SearchableFields)
        {
            var propertyInfo = typeof(T).GetProperty(fieldName);
            if (propertyInfo == null || propertyInfo.PropertyType != typeof(string))
            {
                continue;
            }

            var propertyExpression = Expression.Property(parameter, propertyInfo);

            // Build expression tree: setweight(to_tsvector('language', coalesce(field, '')), 'weight')
            // This creates an Expression.Call for each PostgreSQL function
            // EF Core will translate this to SQL - the C# methods are NEVER called

            // Step 1: coalesce(field, '') - handles null values
            // Creates: Expression representing PostgreSqlFunctions.Coalesce(field, "")
            var coalesceCall = Expression.Call(
                typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.Coalesce))!,
                propertyExpression,
                Expression.Constant(string.Empty));

            // Step 2: to_tsvector('language', coalesce(...)) - convert text to searchable vector
            // Creates: Expression representing PostgreSqlFunctions.ToTsVector("english", ...)
            var toTsVectorCall = Expression.Call(
                typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.ToTsVector))!,
                Expression.Constant(language),
                coalesceCall);

            // Step 3: setweight(..., 'weight') - assign importance weight (A, B, C, or D)
            // Creates: Expression representing PostgreSqlFunctions.SetWeight(..., "A")
            var setWeightCall = Expression.Call(
                typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.SetWeight))!,
                toTsVectorCall,
                Expression.Constant(weight.ToString()));

            // Step 4: Concatenate with previous field vectors using || operator
            // Creates: Expression representing PostgreSqlFunctions.ConcatTsVectors(vector1, vector2)
            // SQL result: vector1 || vector2
            searchVectorExpression = searchVectorExpression == null
                ? setWeightCall
                : Expression.Call(
                    typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.ConcatTsVectors))!,
                    searchVectorExpression,
                    setWeightCall);
        }

        if (searchVectorExpression == null)
        {
            throw new InvalidOperationException("No searchable fields configured");
        }

        // Now build the tsquery from the user's search term
        // websearch_to_tsquery('english', 'search term') - converts user input to query
        // Supports web search syntax: "quoted phrases", AND, OR, -exclusion
        var tsQueryExpression = Expression.Call(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.WebSearchToTsQuery))!,
            Expression.Constant(language),
            Expression.Constant(sanitizedQuery));

        // Build the relevance scoring expression
        // ts_rank(searchVector, tsQuery) - returns float score (higher = better match)
        // This is used in SELECT to return relevance scores with results
        var tsRankExpression = Expression.Call(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.TsRank))!,
            searchVectorExpression,
            tsQueryExpression);

        // Build the matching expression for filtering
        // searchVector @@ tsQuery - boolean match (filters to only matching rows)
        // This is used in WHERE clause
        var matchesExpression = Expression.Call(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.Matches))!,
            searchVectorExpression,
            tsQueryExpression);

        // Create lambda expression: x => Matches(searchVector, tsQuery)
        // EF Core will translate this to: WHERE ... @@ ...
        var filterLambda = Expression.Lambda<Func<T, bool>>(matchesExpression, parameter);

        // Apply the WHERE filter to the query
        // At this point, EF Core will translate all the expression trees to SQL
        var filteredQuery = query.Where(filterLambda);

        // Now build the SELECT projection to include relevance scores
        // We need to rebuild the search vector expression with a new parameter for the Select clause
        // This allows us to do: SELECT entity, ts_rank(searchVector, query) AS Score
        var newParameter = Expression.Parameter(typeof(T), "x");

        // Rebuild the entire search vector expression for the SELECT clause
        // (We can't reuse the previous one because it has a different parameter)
        Expression? newSearchVectorExpression = null;
        foreach (var (fieldName, weight) in configuration.SearchableFields)
        {
            var propertyInfo = typeof(T).GetProperty(fieldName);
            if (propertyInfo == null || propertyInfo.PropertyType != typeof(string))
            {
                continue;
            }

            var propertyExpression = Expression.Property(newParameter, propertyInfo);

            var coalesceCall = Expression.Call(
                typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.Coalesce))!,
                propertyExpression,
                Expression.Constant(string.Empty));

            var toTsVectorCall = Expression.Call(
                typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.ToTsVector))!,
                Expression.Constant(language),
                coalesceCall);

            var setWeightCall = Expression.Call(
                typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.SetWeight))!,
                toTsVectorCall,
                Expression.Constant(weight.ToString()));

            newSearchVectorExpression = newSearchVectorExpression == null
                ? setWeightCall
                : Expression.Call(
                    typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.ConcatTsVectors))!,
                    newSearchVectorExpression,
                    setWeightCall);
        }

        // Create ts_rank expression with new parameter
        var newTsRankExpression = Expression.Call(
            typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.TsRank))!,
            newSearchVectorExpression!,
            tsQueryExpression);

        // Create the member bindings for SearchItemWithScore
        var entityBinding = Expression.Bind(
            typeof(SearchItemWithScore<T>).GetProperty(nameof(SearchItemWithScore<T>.Entity))!,
            newParameter);

        var scoreBinding = Expression.Bind(
            typeof(SearchItemWithScore<T>).GetProperty(nameof(SearchItemWithScore<T>.Score))!,
            newTsRankExpression);

        var memberInit = Expression.MemberInit(
            Expression.New(typeof(SearchItemWithScore<T>)),
            entityBinding,
            scoreBinding);

        var selectLambda = Expression.Lambda<Func<T, SearchItemWithScore<T>>>(memberInit, newParameter);

        // Apply the SELECT projection
        // EF Core will translate this entire expression tree to SQL
        var scoredQuery = filteredQuery.Select(selectLambda);

        // FINAL SQL GENERATED (example for Users table with "developer" search):
        // SELECT
        //     u."Id", u."Email", u."FirstName", u."LastName", ...,
        //     ts_rank(
        //         setweight(to_tsvector('english', coalesce(u."Email", '')), 'A') ||
        //         setweight(to_tsvector('english', coalesce(u."FirstName", '')), 'B') ||
        //         setweight(to_tsvector('english', coalesce(u."LastName", '')), 'B'),
        //         websearch_to_tsquery('english', 'developer')
        //     ) AS Score
        // FROM "Users" AS u
        // WHERE (
        //     setweight(to_tsvector('english', coalesce(u."Email", '')), 'A') ||
        //     setweight(to_tsvector('english', coalesce(u."FirstName", '')), 'B') ||
        //     setweight(to_tsvector('english', coalesce(u."LastName", '')), 'B')
        // ) @@ websearch_to_tsquery('english', 'developer')
        // ORDER BY Score DESC
        //
        // All PostgreSQL function calls are executed in the database, not in C#!

        return scoredQuery;
    }

    private IQueryable<SearchItemWithScore<T>> ApplySortingToScoredQuery<T>(
        IQueryable<SearchItemWithScore<T>> query,
        List<SortCriteria>? sorts,
        SearchConfiguration<T> configuration) where T : class
    {
        // Always sort by relevance score first (descending)
        var orderedQuery = query.OrderByDescending(x => x.Score);

        // Then apply additional sorting on the entity
        if (sorts != null && sorts.Count > 0)
        {
            foreach (var sort in sorts)
            {
                if (!configuration.SortableFields.Contains(sort.Field))
                {
                    continue; // Skip invalid fields
                }

                var propertyInfo = typeof(T).GetProperty(sort.Field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    continue;
                }

                var parameter = Expression.Parameter(typeof(SearchItemWithScore<T>), "x");
                var entityProperty = Expression.Property(parameter, nameof(SearchItemWithScore<T>.Entity));
                var propertyExpression = Expression.Property(entityProperty, propertyInfo);
                var lambda = Expression.Lambda(propertyExpression, parameter);

                var methodName = sort.Direction == SortDirection.Ascending ? "ThenBy" : "ThenByDescending";
                var method = typeof(Queryable).GetMethods()
                    .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(SearchItemWithScore<T>), propertyInfo.PropertyType);

                orderedQuery = (IOrderedQueryable<SearchItemWithScore<T>>)method.Invoke(null, new object[] { orderedQuery, lambda })!;
            }
        }

        return orderedQuery;
    }
}

/// <summary>
/// Internal class to hold entity with search relevance score
/// </summary>
internal class SearchItemWithScore<T> where T : class
{
    public T Entity { get; set; } = default!;
    public double Score { get; set; }
}
