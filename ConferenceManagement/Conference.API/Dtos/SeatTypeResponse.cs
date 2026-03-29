namespace Conference.API.Dtos
{
    public record SeatTypeResponse(
        int Id,
        string Name,
        Guid ConferenceId,
        int Quota
    );
}
