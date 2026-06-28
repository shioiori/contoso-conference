using System.Security.Cryptography;
using System.Text;

namespace Eventbox.EventManagement.EventApi.Application.Extensions
{
    public static class AccessCodeExtension
    {
        public static string Hash(this string accessCode)
        {
            if (string.IsNullOrWhiteSpace(accessCode))
                return string.Empty;
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(accessCode.Trim()));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
        public static string Generate()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string([.. Enumerable.Range(0, 5).Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])]);
        }
    }
}
