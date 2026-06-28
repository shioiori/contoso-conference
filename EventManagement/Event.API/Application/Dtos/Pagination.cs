using System.ComponentModel.DataAnnotations;

namespace Eventbox.EventManagement.EventApi.Application.Dtos
{
    public class PagingInfo
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0.")]
        public int Page { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0.")]
        public int PageSize { get; set; }
    }

    public class PagedResult<T> : PagingInfo
    {
        public IReadOnlyList<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
