namespace Eventbox.EventManagement.EventApi.Dtos
{
    public class SeatTypeResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public Guid EventId { get; set; }
        public int Quota { get; set; }
    }
}
