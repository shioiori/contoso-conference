namespace Eventbox.EventManagement.EventApi.Application.Dtos
{
    public class PagingInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class PagedResult<T> : PagingInfo
    {
        public IReadOnlyList<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
