using Shared.Common.Entities;

namespace IdentityService.Domain.Entities
{
    /// <summary>
    /// Represents a role that can be assigned to users, composed of one or more features.
    /// </summary>
    public sealed class Role : BaseEntity
    {
        /// <summary>Gets or sets the primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets the role name.</summary>
        public string Name { get; set; } = string.Empty;

       

        /// <summary>Gets or sets the collection of role-feature mappings for this role.</summary>
        public ICollection<RoleFeature> RoleFeatures { get; set; } = new List<RoleFeature>();

        /// <summary>Gets or sets the collection of users assigned to this role.</summary>
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}