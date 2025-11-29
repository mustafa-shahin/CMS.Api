namespace CMS.Application.Common.Models
{
    public sealed record PaginationParams
    {
        private const int MaxPageSize = 100;
        private const int DefaultPageSize = 10;

        public int PageNumber { get; init; } = 1;

        private int _pageSize = DefaultPageSize;
        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        public string? SearchTerm { get; init; }
        public string? SortBy { get; init; }
        public bool SortDescending { get; init; }
    }
}
