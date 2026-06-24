using System.ComponentModel.DataAnnotations.Schema;

namespace Eventbox.EventManagement.EventApi.Domains
{
    [Table("OrganizationMembers")]
    public class OrganizationMember
    {
        public Guid OrganizationId { get; set; }
        public Guid OrganizerId { get; set; }

        public OrganizationMember(Guid organizationId, Guid organizerId)
        {
            OrganizationId = organizationId;
            OrganizerId = organizerId;
        }

        private OrganizationMember() { }
    }
}
