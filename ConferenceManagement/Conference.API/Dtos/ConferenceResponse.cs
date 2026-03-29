namespace Conference.API.Dtos
{
    public record ConferenceResponse(
        Guid Id,
        string Name,
        string? Description,
        string Slug,
        DateTime StartDate,
        DateTime EndDate,
        bool IsPublished,
        IEnumerable<SeatTypeResponse> Seats
    );
}
