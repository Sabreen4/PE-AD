using Shared.Common.Entities;

namespace IdentityService.Domain.Entities
{
    /// <summary>
    /// Represents a system feature/permission that can be assigned to roles.
    /// </summary>
    public sealed class Feature : BaseEntity
    {
        /// <summary>Gets or sets the primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets the display name of the feature.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the unique key identifier for the feature.</summary>
        public string Key { get; set; } = string.Empty;

        

        
        /// <summary>Gets or sets the collection of role-feature mappings for this feature.</summary>
        public ICollection<RoleFeature> RoleFeatures { get; set; } = new List<RoleFeature>();
    }
}