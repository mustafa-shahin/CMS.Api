# CMS Search Functionality Documentation

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [How It Works](#how-it-works)
4. [PostgreSQL Full-Text Search](#postgresql-full-text-search)
5. [API Usage](#api-usage)
6. [Search Features](#search-features)
7. [Security](#security)
8. [Configuration](#configuration)
9. [Adding Search to New Entities](#adding-search-to-new-entities)
10. [Performance Optimization](#performance-optimization)
11. [Troubleshooting](#troubleshooting)

---

## Overview

The CMS Search functionality provides a comprehensive, secure, and extensible search system with the following capabilities:

- **PostgreSQL Full-Text Search**: Native database-level full-text search with relevance scoring
- **Advanced Filtering**: Support for multiple filter operators (equals, contains, greater than, less than, in, between, etc.)
- **Multi-Field Sorting**: Sort by multiple fields with custom directions
- **Pagination**: Efficient pagination with configurable page sizes
- **Weighted Search**: Assign importance weights to different fields (A, B, C, D)
- **Complex Queries**: Support for joined tables and complex filtering logic
- **Security First**: SQL injection prevention, input validation, and rate limiting
- **Extensible Design**: Easy to add search to new entities with minimal code

### Current Search Endpoints

- `POST /api/v1/users/search` - Search users
- `POST /api/v1/pages/search` - Search pages
- `POST /api/v1/media/search` - Search images

---

## Architecture

The search system follows Clean Architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                      API Layer                               │
│  Controllers (UsersController, PagesController, etc.)       │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                  Application Layer                           │
│                                                               │
│  ├── Queries                                                 │
│  │   ├── SearchUsersQuery                                   │
│  │   ├── SearchPagesQuery                                   │
│  │   └── SearchImagesQuery                                  │
│  │                                                            │
│  ├── Services                                                │
│  │   ├── ISearchService                                     │
│  │   └── SearchService (Core Search Logic)                 │
│  │                                                            │
│  ├── Models                                                  │
│  │   ├── SearchRequest                                      │
│  │   ├── SearchResult                                       │
│  │   ├── FilterCriteria                                     │
│  │   └── SortCriteria                                       │
│  │                                                            │
│  ├── Extensions                                              │
│  │   ├── PostgreSqlFunctions (EF Core DbFunctions)         │
│  │   ├── SearchConfiguration                                │
│  │   └── DynamicQueryBuilder                                │
│  │                                                            │
│  └── Validators                                              │
│      └── SearchRequestValidator                             │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│              Infrastructure Layer                            │
│                                                               │
│  ├── ApplicationDbContext                                   │
│  │   └── PostgreSQL Function Mappings                       │
│  │                                                            │
│  └── Migrations                                              │
│      ├── AddFullTextSearchFunctions                         │
│      └── AddFullTextSearchIndexes                           │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                   PostgreSQL Database                        │
│                                                               │
│  ├── Full-Text Search Functions                             │
│  │   ├── to_tsvector()                                      │
│  │   ├── websearch_to_tsquery()                             │
│  │   ├── ts_rank()                                          │
│  │   ├── setweight()                                        │
│  │   └── Custom Wrapper Functions                           │
│  │                                                            │
│  └── GIN Indexes (for performance)                          │
│      ├── idx_users_search_vector                            │
│      ├── idx_pages_search_vector                            │
│      └── idx_images_search_vector                           │
└─────────────────────────────────────────────────────────────┘
```

---

## How It Works

### 1. Request Flow

```
User Request → Controller → MediatR → Query Handler → SearchService → EF Core → PostgreSQL
                                                                                     ↓
User Response ← Controller ← MediatR ← Query Handler ← SearchService ← EF Core ← Results
```

### 2. SearchService Core Logic

The `SearchService` is the heart of the search functionality:

```csharp
public async Task<SearchResult<T>> SearchAsync<T>(
    IQueryable<T> baseQuery,
    SearchRequest request,
    SearchConfiguration<T> configuration,
    CancellationToken cancellationToken = default)
{
    // Step 1: Validate and sanitize request
    var validatedRequest = ValidateRequest(request, configuration);

    // Step 2: Apply filters using DynamicQueryBuilder
    var filterExpression = DynamicQueryBuilder.BuildFilterExpression<T>(
        validatedRequest.Filters,
        configuration.FilterableFields.Keys.ToHashSet());

    // Step 3: Check if full-text search is needed
    var useFullTextSearch = !string.IsNullOrWhiteSpace(validatedRequest.SearchTerm) &&
                            validatedRequest.SearchTerm.Length >= configuration.MinSearchTermLength &&
                            configuration.SearchableFields.Count > 0;

    if (useFullTextSearch)
    {
        // Step 4: Apply PostgreSQL full-text search with relevance scoring
        scoredQuery = ApplyFullTextSearchWithScoring(query, validatedRequest, configuration);

        // Step 5: Filter by minimum relevance score
        // Step 6: Apply sorting (by relevance score first, then custom sorts)
        // Step 7: Apply pagination
        // Step 8: Execute query and return results with scores
    }
    else
    {
        // Alternative path: Just filtering and sorting without full-text search
        // Step 4: Count total results
        // Step 5: Apply sorting
        // Step 6: Apply pagination
        // Step 7: Execute query and return results
    }
}
```

### 3. Expression Tree Building

The search system uses **C# Expression Trees** to build dynamic LINQ queries that EF Core translates to SQL:

```csharp
// Example: Building a search vector expression dynamically
var parameter = Expression.Parameter(typeof(User), "x");

// For each searchable field, build: setweight(to_tsvector('english', coalesce(field, '')), 'weight')
var propertyExpression = Expression.Property(parameter, "Email");

// Step 1: coalesce(Email, '') - handle nulls
var coalesceCall = Expression.Call(
    typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.Coalesce))!,
    propertyExpression,
    Expression.Constant(string.Empty));

// Step 2: to_tsvector('english', coalesce(...))
var toTsVectorCall = Expression.Call(
    typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.ToTsVector))!,
    Expression.Constant("english"),
    coalesceCall);

// Step 3: setweight(..., 'A') - assign importance
var setWeightCall = Expression.Call(
    typeof(PostgreSqlFunctions).GetMethod(nameof(PostgreSqlFunctions.SetWeight))!,
    toTsVectorCall,
    Expression.Constant("A"));

// EF Core translates this expression tree to SQL - the C# methods are NEVER executed!
```

---

## PostgreSQL Full-Text Search

### What is Full-Text Search?

PostgreSQL's full-text search allows you to search natural language documents and rank them by relevance. Unlike `LIKE '%term%'` searches, full-text search:

- **Understands language**: Handles stemming (e.g., "running" matches "run")
- **Ranks results**: Returns results sorted by relevance
- **Performs efficiently**: Uses specialized indexes (GIN) for fast searching
- **Supports operators**: AND, OR, NOT, phrase searches, etc.

### Key PostgreSQL Functions

#### 1. `to_tsvector(config, text)`

Converts text to a searchable vector (tsvector):

```sql
-- Example
SELECT to_tsvector('english', 'The quick brown fox jumps');
-- Result: 'brown':3 'fox':4 'jump':5 'quick':2
```

**Note**: Words are stemmed (e.g., "jumps" → "jump") and stop words (e.g., "the") are removed.

#### 2. `websearch_to_tsquery(config, query)`

Converts a web search query to a tsquery with support for:
- Quoted phrases: `"brown fox"`
- AND operator: `quick AND fox` or just `quick fox`
- OR operator: `quick OR slow`
- NOT operator: `-slow` or `NOT slow`

```sql
-- Example
SELECT websearch_to_tsquery('english', '"brown fox" OR quick');
-- Result: 'brown' <-> 'fox' | 'quick'
```

#### 3. `ts_rank(vector, query)`

Ranks how well a document matches a query (returns float, higher = better match):

```sql
-- Example
SELECT ts_rank(
    to_tsvector('english', 'The quick brown fox'),
    websearch_to_tsquery('english', 'brown fox')
);
-- Result: 0.0607927 (relevance score)
```

#### 4. `setweight(vector, weight)`

Assigns importance weights to fields (A, B, C, D where A = highest):

```sql
-- Example: Title is more important than description
SELECT
    setweight(to_tsvector('english', title), 'A') ||
    setweight(to_tsvector('english', description), 'C')
FROM articles;
```

#### 5. `@@` (Match Operator)

Checks if a tsvector matches a tsquery (returns boolean):

```sql
-- Example
SELECT * FROM users
WHERE to_tsvector('english', email) @@ websearch_to_tsquery('english', 'developer');
```

### Generated SQL Example

When you search for "developer" in users, the SearchService generates:

```sql
SELECT
    u."Id", u."Email", u."FirstName", u."LastName", ...,
    ts_rank(
        setweight(to_tsvector('english', coalesce(u."Email", '')), 'A') ||
        setweight(to_tsvector('english', coalesce(u."FirstName", '')), 'B') ||
        setweight(to_tsvector('english', coalesce(u."LastName", '')), 'B'),
        websearch_to_tsquery('english', 'developer')
    ) AS "Score"
FROM "Users" AS u
WHERE (
    setweight(to_tsvector('english', coalesce(u."Email", '')), 'A') ||
    setweight(to_tsvector('english', coalesce(u."FirstName", '')), 'B') ||
    setweight(to_tsvector('english', coalesce(u."LastName", '')), 'B')
) @@ websearch_to_tsquery('english', 'developer')
ORDER BY "Score" DESC
LIMIT 10 OFFSET 0;
```

### EF Core DbFunctions Pattern

**IMPORTANT**: The methods in `PostgreSqlFunctions.cs` are NOT meant to be called directly in C# code!

```csharp
public static class PostgreSqlFunctions
{
    // This method is NEVER executed in C# - it's only for EF Core translation
    public static string ToTsVector(string config, string text)
    {
        throw new InvalidOperationException(
            "This method is for use in LINQ queries only and will be translated to SQL by EF Core.");
    }
}
```

**How it works**:

1. Methods are declared with signatures matching PostgreSQL functions
2. In `ApplicationDbContext.OnModelCreating`, they're registered via `HasDbFunction`
3. When used in LINQ queries, EF Core translates them to actual PostgreSQL SQL
4. The C# method bodies are NEVER executed - they just throw exceptions if called directly

**Example usage** (correct):

```csharp
// In a LINQ query - EF Core translates to SQL
var query = dbContext.Users.Where(u =>
    PostgreSqlFunctions.Matches(
        PostgreSqlFunctions.ToTsVector("english", u.Email),
        PostgreSqlFunctions.WebSearchToTsQuery("english", "developer")
    ));
// EF Core translates to:
// WHERE to_tsvector('english', "Email") @@ websearch_to_tsquery('english', 'developer')
```

**Example** (WRONG - will throw exception):

```csharp
// Direct C# call - DON'T DO THIS!
var result = PostgreSqlFunctions.ToTsVector("english", "text"); // THROWS EXCEPTION
```

---

## API Usage

### Search Request Model

```json
{
  "pageNumber": 1,
  "pageSize": 10,
  "searchTerm": "developer",
  "searchFields": ["Email", "FirstName"],
  "minRelevanceScore": 0.1,
  "filters": [
    {
      "field": "Role",
      "operator": "Equals",
      "value": "Admin",
      "logicalOperator": "And"
    },
    {
      "field": "IsActive",
      "operator": "Equals",
      "value": true,
      "logicalOperator": "And"
    }
  ],
  "sorts": [
    {
      "field": "CreatedAt",
      "direction": "Descending"
    }
  ],
  "includeDeleted": false
}
```

### Filter Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `Equals` | Exact match | `"Role" equals "Admin"` |
| `NotEquals` | Not equal to | `"Status" not equals "Deleted"` |
| `Contains` | String contains | `"Email" contains "gmail"` |
| `NotContains` | String doesn't contain | `"Email" not contains "spam"` |
| `StartsWith` | String starts with | `"FirstName" starts with "John"` |
| `EndsWith` | String ends with | `"Email" ends with ".com"` |
| `GreaterThan` | Greater than | `"Age" > 18` |
| `GreaterThanOrEqual` | Greater than or equal | `"Price" >= 100` |
| `LessThan` | Less than | `"Stock" < 10` |
| `LessThanOrEqual` | Less than or equal | `"Discount" <= 50` |
| `In` | Value in list | `"Status" in ["Active", "Pending"]` |
| `NotIn` | Value not in list | `"Role" not in ["Guest", "Banned"]` |
| `IsNull` | Field is null | `"DeletedAt" is null` |
| `IsNotNull` | Field is not null | `"PublishedAt" is not null` |
| `Between` | Value between two values | `"CreatedAt" between ["2024-01-01", "2024-12-31"]` |

### Search Response Model

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "data": {
          "id": "123",
          "email": "developer@example.com",
          "firstName": "John",
          "lastName": "Doe"
        },
        "relevanceScore": 0.8534,
        "highlights": {
          "email": "Found '<mark>developer</mark>' in email"
        }
      }
    ],
    "totalCount": 42,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "hasPreviousPage": false,
    "hasNextPage": true,
    "executionTimeMs": 23,
    "appliedFilters": [...],
    "appliedSorts": [...],
    "searchTerm": "developer"
  },
  "message": "Search completed successfully"
}
```

### Example API Calls

#### 1. Simple Text Search

```bash
curl -X POST "https://api.example.com/api/v1/users/search" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "searchTerm": "developer",
    "pageNumber": 1,
    "pageSize": 10
  }'
```

#### 2. Search with Filters

```bash
curl -X POST "https://api.example.com/api/v1/users/search" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "searchTerm": "john",
    "filters": [
      {
        "field": "Role",
        "operator": "Equals",
        "value": "Admin"
      },
      {
        "field": "IsActive",
        "operator": "Equals",
        "value": true
      }
    ],
    "pageNumber": 1,
    "pageSize": 20
  }'
```

#### 3. Search with Sorting

```bash
curl -X POST "https://api.example.com/api/v1/pages/search" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "searchTerm": "documentation",
    "sorts": [
      {
        "field": "PublishedAt",
        "direction": "Descending"
      },
      {
        "field": "Title",
        "direction": "Ascending"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10
  }'
```

#### 4. Advanced Search with Multiple Filters

```bash
curl -X POST "https://api.example.com/api/v1/pages/search" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "searchTerm": "api guide",
    "minRelevanceScore": 0.3,
    "filters": [
      {
        "field": "Status",
        "operator": "Equals",
        "value": "Published",
        "logicalOperator": "And"
      },
      {
        "field": "PublishedAt",
        "operator": "GreaterThanOrEqual",
        "value": "2024-01-01T00:00:00Z",
        "logicalOperator": "And"
      },
      {
        "field": "Version",
        "operator": "GreaterThan",
        "value": 1
      }
    ],
    "sorts": [
      {
        "field": "PublishedAt",
        "direction": "Descending"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10
  }'
```

#### 5. Filter-Only Search (No Text Search)

```bash
curl -X POST "https://api.example.com/api/v1/users/search" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "filters": [
      {
        "field": "CreatedAt",
        "operator": "Between",
        "value": ["2024-01-01T00:00:00Z", "2024-12-31T23:59:59Z"]
      }
    ],
    "sorts": [
      {
        "field": "CreatedAt",
        "direction": "Descending"
      }
    ],
    "pageNumber": 1,
    "pageSize": 50
  }'
```

---

## Search Features

### 1. Full-Text Search

- **Language Support**: English by default (configurable per entity)
- **Stemming**: Automatically handles word variations (e.g., "running" matches "run")
- **Stop Words**: Ignores common words (e.g., "the", "a", "an")
- **Weighted Fields**: Prioritize important fields (e.g., title > description)
- **Relevance Scoring**: Results sorted by how well they match the query

### 2. Web Search Syntax

Supports Google-like search syntax:

| Syntax | Description | Example |
|--------|-------------|---------|
| `word1 word2` | AND (both must match) | `john developer` |
| `"exact phrase"` | Exact phrase match | `"senior developer"` |
| `word1 OR word2` | Either word matches | `frontend OR backend` |
| `-word` | Exclude word | `developer -junior` |
| `word1 AND word2` | Explicit AND | `developer AND senior` |

### 3. Filtering

- **Multiple Filters**: Combine filters with AND/OR logic
- **Type-Safe**: Validates field names against allowed list
- **15 Filter Operators**: Equals, Contains, GreaterThan, In, Between, etc.
- **Null Handling**: IsNull and IsNotNull operators

### 4. Sorting

- **Multi-Field Sorting**: Sort by multiple fields in priority order
- **Relevance First**: When using full-text search, results are sorted by relevance score first, then custom sorts
- **Ascending/Descending**: Control sort direction per field

### 5. Pagination

- **Configurable Page Size**: Default 10, max 100 items per page
- **Navigation Helpers**: `HasPreviousPage`, `HasNextPage`, `TotalPages`
- **Efficient**: Uses `OFFSET` and `LIMIT` in SQL

### 6. Performance Metrics

- **Execution Time**: Each response includes query execution time in milliseconds
- **Transparency**: Applied filters and sorts are returned in response

---

## Security

### 1. SQL Injection Prevention

**Input Sanitization**:
```csharp
public static string SanitizeSearchQuery(this string? query)
{
    if (string.IsNullOrWhiteSpace(query))
        return string.Empty;

    // Remove dangerous SQL patterns
    var sanitized = query
        .Replace("'", "")  // Remove single quotes
        .Replace(";", "")  // Remove semicolons
        .Replace("--", "") // Remove SQL comments
        .Replace("/*", "") // Remove block comments
        .Replace("*/", "")
        .Trim();

    // Limit length
    return sanitized.Length > 200 ? sanitized[..200] : sanitized;
}
```

**Parameterized Queries**: All queries use EF Core's parameterization, never string concatenation.

**Field Validation**: Only allowed fields can be filtered/sorted:
```csharp
var filterExpression = DynamicQueryBuilder.BuildFilterExpression<T>(
    validatedRequest.Filters,
    configuration.FilterableFields.Keys.ToHashSet()); // Only allowed fields
```

### 2. Input Validation

**FluentValidation** rules:

```csharp
public sealed class SearchRequestValidator : AbstractValidator<SearchRequest>
{
    public SearchRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.SearchTerm).MaximumLength(200).Must(BeValidSearchTerm);

        // Validate filters and sorts
        // ...
    }

    private bool BeValidSearchTerm(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return true;

        var dangerousChars = new[] { ";", "--", "/*", "*/", "xp_", "sp_", "<script", "<iframe" };
        var lowerTerm = searchTerm.ToLower();
        return !dangerousChars.Any(dangerous => lowerTerm.Contains(dangerous));
    }
}
```

### 3. Rate Limiting

Configured in API layer:
- Global rate limit: 100 requests per minute
- Per-endpoint limits configurable

### 4. Authorization

All search endpoints require authentication:
```csharp
[Authorize]
[HttpPost("search")]
public async Task<IActionResult> SearchUsers(...)
```

Role-based access control can be added per endpoint.

### 5. Data Exposure Control

- **DTOs**: Search results return DTOs, not domain entities (prevents over-posting)
- **Field Whitelisting**: Only configured fields are searchable/filterable
- **Soft Delete Support**: `IncludeDeleted` flag (default false) prevents accidental exposure of deleted records

---

## Configuration

### SearchConfiguration

Each entity defines its search configuration:

```csharp
var searchConfig = new SearchConfiguration<User>()
    // Searchable fields with weights (A = highest, D = lowest)
    .AddSearchableField(u => u.Email, 'A')       // Most important
    .AddSearchableField(u => u.FirstName, 'B')   // High importance
    .AddSearchableField(u => u.LastName, 'B')    // High importance

    // Filterable fields with allowed operators
    .AddFilterableField(u => u.Email, "Equals", "Contains", "StartsWith")
    .AddFilterableField(u => u.Role, "Equals", "In")
    .AddFilterableField(u => u.IsActive, "Equals")
    .AddFilterableField(u => u.CreatedAt, "GreaterThan", "LessThan", "Between")

    // Sortable fields
    .AddSortableField(u => u.Email)
    .AddSortableField(u => u.FirstName)
    .AddSortableField(u => u.LastName)
    .AddSortableField(u => u.CreatedAt)

    // Defaults
    .SetDefaultSort(u => u.CreatedAt, descending: true)
    .SetMinSearchTermLength(2)
    .SetDefaultPageSize(10)
    .SetMaxPageSize(100)
    .SetLanguage("english");
```

### Field Weights

PostgreSQL full-text search supports 4 weight levels:

| Weight | Priority | Use Case |
|--------|----------|----------|
| **A** | Highest | Primary identifiers (e.g., title, email, name) |
| **B** | High | Important content (e.g., slug, category) |
| **C** | Medium | Secondary content (e.g., meta title, tags) |
| **D** | Low | Additional content (e.g., description, notes) |

**Example**: Searching for "developer" in a user's email (weight A) will rank higher than finding it in their bio (weight D).

### Language Configuration

PostgreSQL supports multiple text search configurations:

```csharp
.SetLanguage("english")  // Default
// Other options: "simple", "spanish", "french", "german", etc.
```

**Simple** language config doesn't stem words or remove stop words (useful for technical terms).

---

## Adding Search to New Entities

### Step-by-Step Guide

Let's add search to a hypothetical `Article` entity:

#### 1. Create the Search Query

**File**: `CMS.Application/Features/Articles/Queries/SearchArticlesQuery.cs`

```csharp
using CMS.Application.Common.Models.Search;
using CMS.Application.Common.Services;
using CMS.Domain.Entities;
using MediatR;

namespace CMS.Application.Features.Articles.Queries;

public sealed record SearchArticlesQuery : SearchRequest, IRequest<SearchResult<ArticleListDto>>
{
}

public sealed class SearchArticlesQueryValidator : AbstractValidator<SearchArticlesQuery>
{
    public SearchArticlesQueryValidator()
    {
        Include(new SearchRequestValidator());

        When(x => x.Filters != null, () =>
        {
            RuleFor(x => x.Filters)
                .Must(filters => filters!.All(f => IsAllowedField(f.Field)))
                .WithMessage("One or more filter fields are not allowed for Article search");
        });

        When(x => x.Sorts != null, () =>
        {
            RuleFor(x => x.Sorts)
                .Must(sorts => sorts!.All(s => IsAllowedSortField(s.Field)))
                .WithMessage("One or more sort fields are not allowed for Article search");
        });
    }

    private static bool IsAllowedField(string field)
    {
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(Article.Title),
            nameof(Article.Slug),
            nameof(Article.Category),
            nameof(Article.Status),
            nameof(Article.PublishedAt),
            nameof(Article.AuthorId),
            nameof(Article.CreatedAt)
        };

        return allowedFields.Contains(field);
    }

    private static bool IsAllowedSortField(string field)
    {
        var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            nameof(Article.Title),
            nameof(Article.PublishedAt),
            nameof(Article.CreatedAt),
            nameof(Article.ViewCount)
        };

        return allowedFields.Contains(field);
    }
}

public sealed class SearchArticlesQueryHandler : IRequestHandler<SearchArticlesQuery, SearchResult<ArticleListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISearchService _searchService;

    public SearchArticlesQueryHandler(IApplicationDbContext context, ISearchService searchService)
    {
        _context = context;
        _searchService = searchService;
    }

    public async Task<SearchResult<ArticleListDto>> Handle(
        SearchArticlesQuery request,
        CancellationToken cancellationToken)
    {
        // Configure search for Article entity
        var searchConfig = new SearchConfiguration<Article>()
            .AddSearchableField(a => a.Title, 'A')        // Highest weight
            .AddSearchableField(a => a.Slug, 'B')         // High weight
            .AddSearchableField(a => a.Summary, 'C')      // Medium weight
            .AddSearchableField(a => a.Content, 'D')      // Low weight
            .AddFilterableField(a => a.Title, "Equals", "Contains", "StartsWith")
            .AddFilterableField(a => a.Category, "Equals", "In")
            .AddFilterableField(a => a.Status, "Equals", "In")
            .AddFilterableField(a => a.PublishedAt, "GreaterThan", "LessThan", "Between", "IsNull", "IsNotNull")
            .AddFilterableField(a => a.AuthorId, "Equals", "In")
            .AddFilterableField(a => a.CreatedAt, "GreaterThan", "LessThan", "Between")
            .AddSortableField(a => a.Title)
            .AddSortableField(a => a.PublishedAt)
            .AddSortableField(a => a.CreatedAt)
            .AddSortableField(a => a.ViewCount)
            .SetDefaultSort(a => a.PublishedAt, descending: true);

        // Base query with includes
        var query = _context.Articles
            .AsNoTracking()
            .Include(a => a.Author);

        // Execute search
        var searchResult = await _searchService.SearchAsync(
            query,
            request,
            searchConfig,
            cancellationToken);

        // Project to DTO
        var dtoResult = new SearchResult<ArticleListDto>
        {
            Items = searchResult.Items.Select(item => new SearchResultItem<ArticleListDto>
            {
                Data = new ArticleListDto
                {
                    Id = item.Data.Id,
                    Title = item.Data.Title,
                    Slug = item.Data.Slug,
                    Summary = item.Data.Summary,
                    Category = item.Data.Category,
                    Status = item.Data.Status,
                    PublishedAt = item.Data.PublishedAt,
                    AuthorName = item.Data.Author != null
                        ? $"{item.Data.Author.FirstName} {item.Data.Author.LastName}"
                        : null,
                    ViewCount = item.Data.ViewCount,
                    CreatedAt = item.Data.CreatedAt
                },
                RelevanceScore = item.RelevanceScore,
                Highlights = item.Highlights
            }).ToList(),
            TotalCount = searchResult.TotalCount,
            PageNumber = searchResult.PageNumber,
            PageSize = searchResult.PageSize,
            ExecutionTimeMs = searchResult.ExecutionTimeMs,
            AppliedFilters = searchResult.AppliedFilters,
            AppliedSorts = searchResult.AppliedSorts,
            SearchTerm = searchResult.SearchTerm,
            Facets = searchResult.Facets
        };

        return dtoResult;
    }
}
```

#### 2. Create the DTO

**File**: `CMS.Application/Features/Articles/DTOs/ArticleListDto.cs`

```csharp
namespace CMS.Application.Features.Articles.DTOs;

public sealed record ArticleListDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime? PublishedAt { get; init; }
    public string? AuthorName { get; init; }
    public int ViewCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

#### 3. Add Controller Endpoint

**File**: `CMS.Api/Controllers/V1/ArticlesController.cs`

```csharp
using CMS.Application.Common.Models;
using CMS.Application.Common.Models.Search;
using CMS.Application.Features.Articles.DTOs;
using CMS.Application.Features.Articles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class ArticlesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ArticlesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Search articles with full-text search, filtering, sorting, and pagination
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponse<SearchResult<ArticleListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchArticles(
        [FromBody] SearchArticlesQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return Ok(ApiResponse<SearchResult<ArticleListDto>>.SuccessResponse(result));
    }
}
```

#### 4. Create Database Migration for Search Index

```bash
cd CMS.Infrastructure
dotnet ef migrations add AddArticleSearchIndex
```

**File**: `CMS.Infrastructure/Persistence/Migrations/XXXXXX_AddArticleSearchIndex.cs`

```csharp
using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddArticleSearchIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create GIN index for full-text search on articles
        migrationBuilder.Sql(@"
            CREATE INDEX idx_articles_search_vector ON ""Articles"" USING GIN (
                setweight(to_tsvector('english', coalesce(""Title"", '')), 'A') ||
                setweight(to_tsvector('english', coalesce(""Slug"", '')), 'B') ||
                setweight(to_tsvector('english', coalesce(""Summary"", '')), 'C') ||
                setweight(to_tsvector('english', coalesce(""Content"", '')), 'D')
            );
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_articles_search_vector;");
    }
}
```

#### 5. Apply Migration

```bash
dotnet ef database update
```

#### 6. Test the Endpoint

```bash
curl -X POST "https://api.example.com/api/v1/articles/search" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "searchTerm": "introduction to apis",
    "filters": [
      {
        "field": "Status",
        "operator": "Equals",
        "value": "Published"
      },
      {
        "field": "Category",
        "operator": "In",
        "value": ["Technology", "Programming"]
      }
    ],
    "sorts": [
      {
        "field": "PublishedAt",
        "direction": "Descending"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10
  }'
```

---

## Performance Optimization

### 1. GIN Indexes

**What are GIN Indexes?**

GIN (Generalized Inverted Index) is PostgreSQL's index type optimized for full-text search. It indexes the individual words (lexemes) in your text fields.

**Benefits**:
- Extremely fast full-text searches (even on millions of records)
- Supports ranking and complex queries
- Much faster than `LIKE '%term%'` searches

**Trade-offs**:
- Slower writes (index must be updated on every insert/update)
- Uses more disk space
- Worth it for read-heavy applications

**Example**:
```sql
CREATE INDEX idx_users_search_vector ON "Users" USING GIN (
    setweight(to_tsvector('english', coalesce("Email", '')), 'A') ||
    setweight(to_tsvector('english', coalesce("FirstName", '')), 'B') ||
    setweight(to_tsvector('english', coalesce("LastName", '')), 'B')
);
```

### 2. AsNoTracking

All search queries use `.AsNoTracking()` since search is read-only:

```csharp
var query = _context.Users.AsNoTracking()...
```

**Benefits**:
- Faster query execution (no change tracking overhead)
- Lower memory usage
- Better performance for read-only operations

### 3. Projection to DTOs

Search results are projected to DTOs before materialization:

```csharp
Items = searchResult.Items.Select(item => new SearchResultItem<UserListDto>
{
    Data = new UserListDto { ... },
    RelevanceScore = item.RelevanceScore
}).ToList()
```

**Benefits**:
- Only fetch required columns (not entire entity)
- Smaller result sets
- Better performance

### 4. Pagination

Always use pagination - never return all results:

```csharp
query = query.Skip(skip).Take(pageSize);
```

**Benefits**:
- Reduced memory usage
- Faster response times
- Better user experience

### 5. Filtered Indexes

For frequently filtered fields, consider partial indexes:

```sql
-- Index only active users
CREATE INDEX idx_active_users ON "Users" (created_at) WHERE is_active = true;

-- Index only published pages
CREATE INDEX idx_published_pages ON "Pages" (published_at) WHERE status = 'Published';
```

### 6. Monitoring

Use the `ExecutionTimeMs` field in responses to monitor query performance:

```json
{
  "executionTimeMs": 23  // If this is consistently > 100ms, investigate
}
```

**Tools**:
- PostgreSQL `EXPLAIN ANALYZE` for query plans
- Application logging for slow queries
- Database monitoring (pg_stat_statements)

---

## Troubleshooting

### Issue: Search returns no results

**Possible Causes**:
1. Search term is too short (< `MinSearchTermLength`)
2. Search term contains only stop words (e.g., "the", "a")
3. No fields are configured as searchable
4. Language configuration mismatch

**Solution**:
```csharp
// Check configuration
var searchConfig = new SearchConfiguration<User>()
    .AddSearchableField(u => u.Email, 'A')  // Ensure fields are added
    .SetMinSearchTermLength(2);  // Lower if needed
```

### Issue: Relevance scores seem wrong

**Possible Cause**: Field weights not configured correctly

**Solution**:
```csharp
// Assign appropriate weights (A = highest, D = lowest)
.AddSearchableField(u => u.Email, 'A')      // Most important
.AddSearchableField(u => u.FirstName, 'B')  // Less important
.AddSearchableField(u => u.Bio, 'D')        // Least important
```

### Issue: Slow search performance

**Possible Causes**:
1. Missing GIN indexes
2. Too many `Include()` statements (N+1 query problem)
3. Not using `AsNoTracking()`
4. Large page sizes

**Solutions**:
```sql
-- Check if index exists
SELECT indexname FROM pg_indexes WHERE tablename = 'Users' AND indexdef LIKE '%GIN%';

-- Create index if missing
CREATE INDEX idx_users_search_vector ON "Users" USING GIN (...);
```

```csharp
// Use AsNoTracking for read-only queries
var query = _context.Users.AsNoTracking()...

// Limit page size
.SetMaxPageSize(100)

// Use Select projection instead of Include for better performance
```

### Issue: "AsSplitQuery" method not found

**Cause**: Missing `Microsoft.EntityFrameworkCore.Relational` package

**Solution**:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="10.0.1" />
```

### Issue: PostgreSQL function not found error

**Cause**: Database migrations not applied

**Solution**:
```bash
cd CMS.Infrastructure
dotnet ef database update
```

Ensure these migrations exist:
- `AddFullTextSearchFunctions` - Creates wrapper functions
- `AddFullTextSearchIndexes` - Creates GIN indexes

### Issue: Filter/Sort field not allowed

**Cause**: Field not configured in `SearchConfiguration` or validator

**Solution**:
```csharp
// Add to configuration
.AddFilterableField(u => u.Email, "Equals", "Contains")
.AddSortableField(u => u.Email)

// Add to validator
private static bool IsAllowedField(string field)
{
    var allowedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        nameof(User.Email),  // Add field here
        // ...
    };
    return allowedFields.Contains(field);
}
```

### Issue: SQL injection concerns

**Solution**: The system is already protected:
1. All queries use parameterization (EF Core)
2. Input sanitization via `SanitizeSearchQuery()`
3. Field whitelisting (only configured fields allowed)
4. FluentValidation rules

**Verify**:
```csharp
// Input is sanitized
var sanitized = request.SearchTerm!.SanitizeSearchQuery();

// Fields are whitelisted
var filterExpression = DynamicQueryBuilder.BuildFilterExpression<T>(
    validatedRequest.Filters,
    configuration.FilterableFields.Keys.ToHashSet());  // Only allowed fields
```

---

## Best Practices

### 1. Configuration

- **Assign appropriate weights**: A for titles/names, B for slugs/categories, C for summaries, D for descriptions
- **Limit searchable fields**: Only add fields that users need to search (more fields = slower indexing)
- **Set reasonable defaults**: `DefaultPageSize = 10`, `MaxPageSize = 100`
- **Use appropriate language config**: "english" for most cases, "simple" for technical terms

### 2. Security

- **Always validate input**: Use FluentValidation for all search requests
- **Whitelist fields**: Only allow configured fields to be filtered/sorted
- **Sanitize search terms**: Remove dangerous characters
- **Rate limit**: Prevent abuse with rate limiting
- **Authorize**: Require authentication for all search endpoints

### 3. Performance

- **Create GIN indexes**: Essential for fast full-text search
- **Use AsNoTracking**: For read-only search queries
- **Project to DTOs**: Only fetch required data
- **Paginate always**: Never return all results
- **Monitor execution time**: Log slow queries (> 100ms)

### 4. User Experience

- **Return relevance scores**: Help users understand why results are ranked
- **Include highlights**: Show where search terms were found (future feature)
- **Provide facets**: Allow filtering by common values (future feature)
- **Return metadata**: TotalCount, HasNextPage, ExecutionTimeMs, etc.

### 5. Maintenance

- **Keep documentation updated**: Update this file when adding new entities
- **Version your API**: Use API versioning for breaking changes
- **Log search queries**: Analyze what users search for to improve relevance
- **Monitor performance**: Set up alerts for slow queries

---



