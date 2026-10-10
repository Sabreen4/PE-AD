using Shared.Common.Entities;

namespace IdentityService.Domain.Entities
{
    /// <summary>
    /// Junction entity mapping a role to a feature.
    /// </summary>
    public sealed class RoleFeature : BaseEntity
    {
        /// <summary>Gets or sets the primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets the role identifier.</summary>
        public Guid RoleId { get; set; }

        /// <summary>Gets or sets the feature identifier.</summary>
        public Guid FeatureId { get; set; }

        /// <summary>Gets or sets the navigation property to the associated role.</summary>
        public Role Role { get; set; } = null!;

        /// <summary>Gets or sets the navigation property to the associated feature.</summary>
        public Feature Feature { get; set; } = null!;
    }
}