using System.ComponentModel.DataAnnotations.Schema;

namespace Eventbox.EventManagement.EventApi.Domains
{
    [Table("OrganizationMembers")]
    public class OrganizationMember
    {
        public Guid OrganizationId { get; private set; }
        public Guid OrganizerId { get; private set; }

        public OrganizationMember(Guid organizationId, Guid organizerId)
        {
            OrganizationId = organizationId;
            OrganizerId = organizerId;
        }

        private OrganizationMember() { }
    }
}
