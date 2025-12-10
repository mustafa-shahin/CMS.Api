using CMS.Application.Common.Models.Search;
using System.Linq.Expressions;
using System.Reflection;

namespace CMS.Application.Common.Services;

/// <summary>
/// Builds dynamic LINQ expressions for filtering and sorting
/// </summary>
public static class DynamicQueryBuilder
{
    /// <summary>
    /// Build filter expression from filter criteria
    /// </summary>
    public static Expression<Func<T, bool>>? BuildFilterExpression<T>(
        List<FilterCriteria>? filters,
        HashSet<string>? allowedFields = null)
    {
        if (filters == null || filters.Count == 0)
        {
            return null;
        }

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedExpression = null;

        foreach (var filter in filters)
        {
            // Security: Validate field name
            if (allowedFields != null && !allowedFields.Contains(filter.Field, StringComparer.OrdinalIgnoreCase))
            {
                continue; // Skip invalid fields
            }

            var propertyInfo = typeof(T).GetProperty(filter.Field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                continue; // Skip non-existent properties
            }

            var propertyExpression = Expression.Property(parameter, propertyInfo);
            var filterExpression = BuildFilterExpressionForOperator(propertyExpression, filter, propertyInfo.PropertyType);

            if (filterExpression != null)
            {
                combinedExpression = combinedExpression == null
                    ? filterExpression
                    : filter.LogicalOperator == LogicalOperator.And
                        ? Expression.AndAlso(combinedExpression, filterExpression)
                        : Expression.OrElse(combinedExpression, filterExpression);
            }
        }

        return combinedExpression == null
            ? null
            : Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
    }

    private static Expression? BuildFilterExpressionForOperator(
        Expression propertyExpression,
        FilterCriteria filter,
        Type propertyType)
    {
        try
        {
            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            switch (filter.Operator)
            {
                case FilterOperator.Equals:
                    return BuildEqualsExpression(propertyExpression, filter.Value, propertyType);

                case FilterOperator.NotEquals:
                    var equalsExpr = BuildEqualsExpression(propertyExpression, filter.Value, propertyType);
                    return equalsExpr != null ? Expression.Not(equalsExpr) : null;

                case FilterOperator.Contains:
                    return BuildContainsExpression(propertyExpression, filter.Value, propertyType);

                case FilterOperator.NotContains:
                    var containsExpr = BuildContainsExpression(propertyExpression, filter.Value, propertyType);
                    return containsExpr != null ? Expression.Not(containsExpr) : null;

                case FilterOperator.StartsWith:
                    return BuildStringMethodExpression(propertyExpression, filter.Value, "StartsWith", propertyType);

                case FilterOperator.EndsWith:
                    return BuildStringMethodExpression(propertyExpression, filter.Value, "EndsWith", propertyType);

                case FilterOperator.GreaterThan:
                    return BuildComparisonExpression(propertyExpression, filter.Value, ExpressionType.GreaterThan, propertyType);

                case FilterOperator.GreaterThanOrEqual:
                    return BuildComparisonExpression(propertyExpression, filter.Value, ExpressionType.GreaterThanOrEqual, propertyType);

                case FilterOperator.LessThan:
                    return BuildComparisonExpression(propertyExpression, filter.Value, ExpressionType.LessThan, propertyType);

                case FilterOperator.LessThanOrEqual:
                    return BuildComparisonExpression(propertyExpression, filter.Value, ExpressionType.LessThanOrEqual, propertyType);

                case FilterOperator.In:
                    return BuildInExpression(propertyExpression, filter.Value, propertyType);

                case FilterOperator.NotIn:
                    var inExpr = BuildInExpression(propertyExpression, filter.Value, propertyType);
                    return inExpr != null ? Expression.Not(inExpr) : null;

                case FilterOperator.IsNull:
                    return Expression.Equal(propertyExpression, Expression.Constant(null, propertyType));

                case FilterOperator.IsNotNull:
                    return Expression.NotEqual(propertyExpression, Expression.Constant(null, propertyType));

                case FilterOperator.Between:
                    return BuildBetweenExpression(propertyExpression, filter.Value, propertyType);

                default:
                    return null;
            }
        }
        catch
        {
            // Return null if expression building fails (invalid value type, etc.)
            return null;
        }
    }

    private static Expression? BuildEqualsExpression(Expression propertyExpression, object? value, Type propertyType)
    {
        var convertedValue = ConvertValue(value, propertyType);
        if (convertedValue == null && !IsNullableType(propertyType))
        {
            return null;
        }

        var constantExpression = Expression.Constant(convertedValue, propertyType);

        // For strings, use case-insensitive comparison
        if (propertyType == typeof(string))
        {
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            var propertyToLower = Expression.Call(propertyExpression, toLowerMethod);
            var valueToLower = convertedValue != null
                ? Expression.Constant(convertedValue.ToString()!.ToLower(), typeof(string))
                : Expression.Constant(null, typeof(string));
            return Expression.Equal(propertyToLower, valueToLower);
        }

        return Expression.Equal(propertyExpression, constantExpression);
    }

    private static Expression? BuildContainsExpression(Expression propertyExpression, object? value, Type propertyType)
    {
        if (propertyType != typeof(string) || value == null)
        {
            return null;
        }

        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;

        var propertyToLower = Expression.Call(propertyExpression, toLowerMethod);
        var valueConstant = Expression.Constant(value.ToString()!.ToLower(), typeof(string));

        // Check for null first
        var nullCheck = Expression.NotEqual(propertyExpression, Expression.Constant(null, typeof(string)));
        var containsCall = Expression.Call(propertyToLower, containsMethod, valueConstant);

        return Expression.AndAlso(nullCheck, containsCall);
    }

    private static Expression? BuildStringMethodExpression(
        Expression propertyExpression,
        object? value,
        string methodName,
        Type propertyType)
    {
        if (propertyType != typeof(string) || value == null)
        {
            return null;
        }

        var method = typeof(string).GetMethod(methodName, new[] { typeof(string) })!;
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;

        var propertyToLower = Expression.Call(propertyExpression, toLowerMethod);
        var valueConstant = Expression.Constant(value.ToString()!.ToLower(), typeof(string));

        // Check for null first
        var nullCheck = Expression.NotEqual(propertyExpression, Expression.Constant(null, typeof(string)));
        var methodCall = Expression.Call(propertyToLower, method, valueConstant);

        return Expression.AndAlso(nullCheck, methodCall);
    }

    private static Expression? BuildComparisonExpression(
        Expression propertyExpression,
        object? value,
        ExpressionType comparisonType,
        Type propertyType)
    {
        var convertedValue = ConvertValue(value, propertyType);
        if (convertedValue == null)
        {
            return null;
        }

        var constantExpression = Expression.Constant(convertedValue, propertyType);
        return Expression.MakeBinary(comparisonType, propertyExpression, constantExpression);
    }

    private static Expression? BuildInExpression(Expression propertyExpression, object? value, Type propertyType)
    {
        if (value == null)
        {
            return null;
        }

        // Handle collection values
        var enumerable = value as System.Collections.IEnumerable;
        if (enumerable == null)
        {
            return null;
        }

        var values = new List<object>();
        foreach (var item in enumerable)
        {
            var converted = ConvertValue(item, propertyType);
            if (converted != null)
            {
                values.Add(converted);
            }
        }

        if (values.Count == 0)
        {
            return null;
        }

        // Build OR expression: x == value1 || x == value2 || ...
        Expression? combinedExpression = null;
        foreach (var val in values)
        {
            var constantExpression = Expression.Constant(val, propertyType);
            var equalsExpression = Expression.Equal(propertyExpression, constantExpression);

            combinedExpression = combinedExpression == null
                ? equalsExpression
                : Expression.OrElse(combinedExpression, equalsExpression);
        }

        return combinedExpression;
    }

    private static Expression? BuildBetweenExpression(Expression propertyExpression, object? value, Type propertyType)
    {
        if (value == null)
        {
            return null;
        }

        // Expect value to be an array or list with 2 elements [min, max]
        var enumerable = value as System.Collections.IEnumerable;
        if (enumerable == null)
        {
            return null;
        }

        var values = new List<object>();
        foreach (var item in enumerable)
        {
            var converted = ConvertValue(item, propertyType);
            if (converted != null)
            {
                values.Add(converted);
            }
        }

        if (values.Count != 2)
        {
            return null;
        }

        var minConstant = Expression.Constant(values[0], propertyType);
        var maxConstant = Expression.Constant(values[1], propertyType);

        var greaterThanOrEqual = Expression.GreaterThanOrEqual(propertyExpression, minConstant);
        var lessThanOrEqual = Expression.LessThanOrEqual(propertyExpression, maxConstant);

        return Expression.AndAlso(greaterThanOrEqual, lessThanOrEqual);
    }

    /// <summary>
    /// Apply sorting to query
    /// </summary>
    public static IQueryable<T> ApplySorting<T>(
        IQueryable<T> query,
        List<SortCriteria>? sorts,
        HashSet<string>? allowedFields = null,
        string? defaultSortField = null,
        bool defaultSortDescending = false)
    {
        // Use provided sorts or fall back to default
        var effectiveSorts = sorts?.Count > 0 ? sorts : null;

        if (effectiveSorts == null && defaultSortField != null)
        {
            effectiveSorts = new List<SortCriteria>
            {
                new() { Field = defaultSortField, Direction = defaultSortDescending ? SortDirection.Descending : SortDirection.Ascending }
            };
        }

        if (effectiveSorts == null || effectiveSorts.Count == 0)
        {
            return query;
        }

        IOrderedQueryable<T>? orderedQuery = null;

        foreach (var sort in effectiveSorts)
        {
            // Security: Validate field name
            if (allowedFields != null && !allowedFields.Contains(sort.Field, StringComparer.OrdinalIgnoreCase))
            {
                continue; // Skip invalid fields
            }

            var propertyInfo = typeof(T).GetProperty(sort.Field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                continue; // Skip non-existent properties
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyExpression = Expression.Property(parameter, propertyInfo);
            var lambda = Expression.Lambda(propertyExpression, parameter);

            var methodName = orderedQuery == null
                ? (sort.Direction == SortDirection.Ascending ? "OrderBy" : "OrderByDescending")
                : (sort.Direction == SortDirection.Ascending ? "ThenBy" : "ThenByDescending");

            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyInfo.PropertyType);

            query = (IQueryable<T>)method.Invoke(null, new object[] { orderedQuery ?? query, lambda })!;
            orderedQuery = (IOrderedQueryable<T>)query;
        }

        return query;
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
        {
            return null;
        }

        try
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Handle enum
            if (underlyingType.IsEnum)
            {
                if (value is string stringValue)
                {
                    return Enum.Parse(underlyingType, stringValue, true);
                }
                return Enum.ToObject(underlyingType, value);
            }

            // Handle guid
            if (underlyingType == typeof(Guid))
            {
                return value is string guidString ? Guid.Parse(guidString) : (Guid)value;
            }

            // Handle datetime
            if (underlyingType == typeof(DateTime))
            {
                return value is string dateString ? DateTime.Parse(dateString) : Convert.ToDateTime(value);
            }

            // Handle datetimeoffset
            if (underlyingType == typeof(DateTimeOffset))
            {
                return value is string dateOffsetString ? DateTimeOffset.Parse(dateOffsetString) : (DateTimeOffset)value;
            }

            // Use Convert for primitive types
            return Convert.ChangeType(value, underlyingType);
        }
        catch
        {
            return null;
        }
    }

    private static bool IsNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null || !type.IsValueType;
    }
}
