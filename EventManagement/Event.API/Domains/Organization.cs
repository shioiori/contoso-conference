using Eventbox.Shared.Objects;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eventbox.EventManagement.EventApi.Domains
{
    [Table("Organization")]
    public class Organization : AuditableEntity<Guid>
    {
        public string Name { get; set; }

        public Organization(Guid id, string name) : base(id)
        {
            Name = string.Empty;
            SetName(name);
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
