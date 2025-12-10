using Microsoft.EntityFrameworkCore;

namespace CMS.Application.Common.Specifications;

/// <summary>
/// Evaluates specifications and applies them to IQueryable
/// </summary>
public static class SpecificationEvaluator
{
    /// <summary>
    /// Apply specification to query
    /// </summary>
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery,
        ISpecification<T> specification) where T : class
    {
        var query = inputQuery;

        // Apply no tracking
        if (specification.IsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // Apply criteria (WHERE clause)
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply include strings
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply ThenBy (for multiple order clauses)
        if (specification.OrderBy != null || specification.OrderByDescending != null)
        {
            var orderedQuery = (IOrderedQueryable<T>)query;

            foreach (var thenBy in specification.ThenBy)
            {
                orderedQuery = orderedQuery.ThenBy(thenBy);
            }

            foreach (var thenByDescending in specification.ThenByDescending)
            {
                orderedQuery = orderedQuery.ThenByDescending(thenByDescending);
            }

            query = orderedQuery;
        }

        // Apply split query
        if (specification.IsSplitQuery)
        {
            query = query.AsSplitQuery();
        }

        // Apply paging (Skip/Take) - should be last
        if (specification.Skip.HasValue)
        {
            query = query.Skip(specification.Skip.Value);
        }

        if (specification.Take.HasValue)
        {
            query = query.Take(specification.Take.Value);
        }

        return query;
    }
}
