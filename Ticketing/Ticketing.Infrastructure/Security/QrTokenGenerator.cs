using System.Security.Cryptography;
using Eventbox.Ticketing.Application.Abstractions;

namespace Eventbox.Ticketing.Infrastructure.Security;

public class QrTokenGenerator : IQrTokenGenerator
{
    public string Generate()
        => $"ckin_{Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=')}";
}
