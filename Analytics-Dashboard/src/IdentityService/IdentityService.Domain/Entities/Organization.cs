using Shared.Common.Entities;

namespace IdentityService.Domain.Entities
{
    /// <summary>
    /// Represents an organization that users and programs belong to.
    /// </summary>
    public sealed class Organization : BaseEntity
    {
        /// <summary>Gets or sets the primary key.</summary>
        public long Id { get; set; }

        /// <summary>Gets or sets the organization name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the organization email.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the organization phone number.</summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>Gets or sets the city where the organization is located.</summary>
        public string City { get; set; } = string.Empty;

      

       
        /// <summary>Gets or sets the collection of users belonging to this organization.</summary>
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}