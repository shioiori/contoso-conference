namespace Eventbox.Registration.Domain.Entities.OrderAggregate
{
    public class PersonalInfo
    {
        private PersonalInfo()
        {
            Name = string.Empty;
            Email = string.Empty;
        }

        public PersonalInfo(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.", nameof(name));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            Name = name.Trim();
            Email = email.Trim();
        }

        public string Name { get; private set; }
        public string Email { get; private set; }
    }
}
