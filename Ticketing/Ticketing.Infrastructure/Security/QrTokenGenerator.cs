using System.Security.Cryptography;
using Eventbox.TicketingApplication.Abstractions;

namespace Eventbox.TicketingInfrastructure.Security;

public class QrTokenGenerator : IQrTokenGenerator
{
    public string Generate()
        => $"ckin_{Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=')}";
}
