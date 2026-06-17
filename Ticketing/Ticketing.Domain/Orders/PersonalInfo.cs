namespace Eventbox.Ticketing.Domain.Entities.OrderAggregate
{
    using System.Net.Mail;

    public class PersonalInfo
    {
        private PersonalInfo()
        {
            Name = string.Empty;
            Email = string.Empty;
        }

        public PersonalInfo(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            var normalizedEmail = email.Trim();
            if (!IsValidEmail(normalizedEmail))
                throw new ArgumentException("Email format is invalid.", nameof(email));

            Name = name?.Trim() ?? string.Empty;
            Email = normalizedEmail;
        }

        public string Name { get; private set; }
        public string Email { get; private set; }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var address = new MailAddress(email);
                return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
