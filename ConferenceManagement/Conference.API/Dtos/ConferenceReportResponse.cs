namespace Conference.API.Dtos
{
    public record ConferenceReportResponse(
        Guid ConferenceId,
        string Name,
        IEnumerable<SeatTypeReportItem> SeatTypes
    );

    public record SeatTypeReportItem(
        int SeatTypeId,
        string Name,
        int TotalQuota
    );
}
