using CMS.Application.Common.Models.Search;
using FluentValidation;

namespace CMS.Application.Common.Validators;

/// <summary>
/// Validator for SearchRequest
/// </summary>
public sealed class SearchRequestValidator : AbstractValidator<SearchRequest>
{
    public SearchRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must not exceed 100");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(200)
            .WithMessage("Search term must not exceed 200 characters")
            .Must(BeValidSearchTerm)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithMessage("Search term contains invalid characters");

        RuleFor(x => x.MinRelevanceScore)
            .InclusiveBetween(0.0, 1.0)
            .When(x => x.MinRelevanceScore.HasValue)
            .WithMessage("Minimum relevance score must be between 0.0 and 1.0");

        RuleForEach(x => x.Filters)
            .SetValidator(new FilterCriteriaValidator())
            .When(x => x.Filters != null);

        RuleForEach(x => x.Sorts)
            .SetValidator(new SortCriteriaValidator())
            .When(x => x.Sorts != null);

        RuleForEach(x => x.SearchFields)
            .NotEmpty()
            .WithMessage("Search field name cannot be empty")
            .MaximumLength(100)
            .WithMessage("Search field name must not exceed 100 characters")
            .Matches("^[a-zA-Z][a-zA-Z0-9_]*$")
            .WithMessage("Search field name must be a valid identifier")
            .When(x => x.SearchFields != null);
    }

    private bool BeValidSearchTerm(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return true;
        }

        // Disallow potentially dangerous characters and SQL injection patterns
        var dangerousChars = new[] { ";", "--", "/*", "*/", "xp_", "sp_", "<script", "<iframe" };
        var lowerTerm = searchTerm.ToLower();

        return !dangerousChars.Any(dangerous => lowerTerm.Contains(dangerous));
    }
}

/// <summary>
/// Validator for FilterCriteria
/// </summary>
public sealed class FilterCriteriaValidator : AbstractValidator<FilterCriteria>
{
    public FilterCriteriaValidator()
    {
        RuleFor(x => x.Field)
            .NotEmpty()
            .WithMessage("Filter field name is required")
            .MaximumLength(100)
            .WithMessage("Filter field name must not exceed 100 characters")
            .Matches("^[a-zA-Z][a-zA-Z0-9_]*$")
            .WithMessage("Filter field name must be a valid identifier (letters, numbers, underscores only, must start with a letter)");

        RuleFor(x => x.Operator)
            .IsInEnum()
            .WithMessage("Invalid filter operator");

        RuleFor(x => x.LogicalOperator)
            .IsInEnum()
            .WithMessage("Invalid logical operator");

        // Validate value is provided for operators that require it
        RuleFor(x => x.Value)
            .NotNull()
            .When(x => x.Operator != FilterOperator.IsNull && x.Operator != FilterOperator.IsNotNull)
            .WithMessage("Filter value is required for this operator");

        // Validate array value for IN and NOT IN operators
        RuleFor(x => x.Value)
            .Must(BeACollection)
            .When(x => x.Operator == FilterOperator.In || x.Operator == FilterOperator.NotIn)
            .WithMessage("Filter value must be a collection for IN/NOT IN operators");

        // Validate array value for BETWEEN operator
        RuleFor(x => x.Value)
            .Must(BeTwoElementCollection)
            .When(x => x.Operator == FilterOperator.Between)
            .WithMessage("Filter value must be a collection with exactly 2 elements for BETWEEN operator");
    }

    private bool BeACollection(object? value)
    {
        if (value == null)
        {
            return false;
        }

        return value is System.Collections.IEnumerable && value is not string;
    }

    private bool BeTwoElementCollection(object? value)
    {
        if (value == null)
        {
            return false;
        }

        if (value is System.Collections.IEnumerable enumerable && value is not string)
        {
            var count = 0;
            foreach (var _ in enumerable)
            {
                count++;
                if (count > 2)
                {
                    return false;
                }
            }
            return count == 2;
        }

        return false;
    }
}

/// <summary>
/// Validator for SortCriteria
/// </summary>
public sealed class SortCriteriaValidator : AbstractValidator<SortCriteria>
{
    public SortCriteriaValidator()
    {
        RuleFor(x => x.Field)
            .NotEmpty()
            .WithMessage("Sort field name is required")
            .MaximumLength(100)
            .WithMessage("Sort field name must not exceed 100 characters")
            .Matches("^[a-zA-Z][a-zA-Z0-9_]*$")
            .WithMessage("Sort field name must be a valid identifier (letters, numbers, underscores only, must start with a letter)");

        RuleFor(x => x.Direction)
            .IsInEnum()
            .WithMessage("Invalid sort direction");
    }
}
