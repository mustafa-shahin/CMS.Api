using System.Linq.Expressions;

namespace CMS.Application.Common.Specifications;

/// <summary>
/// Base implementation of specification pattern
/// </summary>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification()
    {
    }

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public List<Expression<Func<T, object>>> ThenBy { get; } = new();
    public List<Expression<Func<T, object>>> ThenByDescending { get; } = new();
    public int? Skip { get; private set; }
    public int? Take { get; private set; }
    public bool IsSplitQuery { get; private set; }
    public bool IsNoTracking { get; private set; } = true; // Default to no tracking for performance

    /// <summary>
    /// Add criteria to specification
    /// </summary>
    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = Criteria == null ? criteria : Criteria.AndAlso(criteria);
    }

    /// <summary>
    /// Add include for eager loading
    /// </summary>
    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Add include string for nested eager loading
    /// </summary>
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Set order by (ascending)
    /// </summary>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Set order by descending
    /// </summary>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }

    /// <summary>
    /// Add then by (for multi-field sorting)
    /// </summary>
    protected void ApplyThenBy(Expression<Func<T, object>> thenByExpression)
    {
        ThenBy.Add(thenByExpression);
    }

    /// <summary>
    /// Add then by descending (for multi-field sorting)
    /// </summary>
    protected void ApplyThenByDescending(Expression<Func<T, object>> thenByDescendingExpression)
    {
        ThenByDescending.Add(thenByDescendingExpression);
    }

    /// <summary>
    /// Apply paging
    /// </summary>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    /// <summary>
    /// Enable split query
    /// </summary>
    protected void EnableSplitQuery()
    {
        IsSplitQuery = true;
    }

    /// <summary>
    /// Enable tracking
    /// </summary>
    protected void EnableTracking()
    {
        IsNoTracking = false;
    }
}

/// <summary>
/// Extension methods for combining expressions
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Combine two expressions with AND logic
    /// </summary>
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(left.Body);

        var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(right.Body);

        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(leftBody!, rightBody!), parameter);
    }

    /// <summary>
    /// Combine two expressions with OR logic
    /// </summary>
    public static Expression<Func<T, bool>> OrElse<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(left.Body);

        var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(right.Body);

        return Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(leftBody!, rightBody!), parameter);
    }

    private class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression? Visit(Expression? node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }
}
