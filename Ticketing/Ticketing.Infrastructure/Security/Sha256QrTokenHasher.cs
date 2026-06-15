using System.Security.Cryptography;
using System.Text;
using Eventbox.Ticketing.Application.Abstractions;

namespace Eventbox.Ticketing.Infrastructure.Security;

public class Sha256QrTokenHasher : IQrTokenHasher
{
    public string Hash(string qrToken)
    {
        if (string.IsNullOrWhiteSpace(qrToken))
            throw new ArgumentException("QR token is required.", nameof(qrToken));

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(qrToken.Trim()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
