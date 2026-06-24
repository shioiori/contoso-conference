using Eventbox.EventManagement.EventApi.Domains.Enums;

namespace Eventbox.EventManagement.EventApi.Domains.Extensions
{
    public class EventStatusHelper
    {
        public static EventStatus GetCurrentEventStatus(bool isPublished, DateTimeOffset from) =>
            isPublished ? EventStatus.Published :
            from > DateTimeOffset.UtcNow ? EventStatus.Draft :
            EventStatus.Cancelled;
    }
}
