namespace Eventbox.Ticketing.Application.Abstractions;

public interface IQrTokenHasher
{
    string Hash(string qrToken);
}
