using Eventbox.EventManagement.EventApi.Domains.Common;

namespace Eventbox.EventManagement.EventApi.Domains
{
    public class Organization : Entity<Guid>
    {
        public string Name { get; private set; }

        public Organization(Guid id, string name) : base(id)
        {
            Name = string.Empty;
            SetName(name);
        }

        private Organization()
        {
            Name = string.Empty;
        }

        public void Update(string name)
        {
            SetName(name);
            UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Organization name is required.", nameof(name));

            Name = name.Trim();
        }
    }
}
