using Eventbox.EventManagement.EventApi.Domains.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eventbox.EventManagement.EventApi.Domains
{
    [Table("Organization")]
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
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Organization name is required.", nameof(name));

            Name = name.Trim();
        }
    }
}
