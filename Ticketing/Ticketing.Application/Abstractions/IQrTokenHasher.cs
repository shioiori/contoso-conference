namespace Eventbox.TicketingApplication.Abstractions;

public interface IQrTokenHasher
{
    string Hash(string qrToken);
}
