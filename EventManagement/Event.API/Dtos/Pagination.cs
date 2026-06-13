namespace Eventbox.EventManagement.EventApi.Dtos
{
    public class PagingInfo
    {
        public int Page { get; init; }
        public int PageSize { get; init; }
    }

    public class PagedResult<T> : PagingInfo
    {
        public IReadOnlyList<T> Items { get; init; }
        public int TotalItems { get; init; }
        public int TotalPages { get; init; }
    }
}
