using Shared.Common.Entities;

namespace MetaDataService.Domain.Entities
{
    /// <summary>
    /// Junction entity mapping a reference term to a reference set.
    /// </summary>
    public sealed class SetRefTerm : BaseEntity
    {
        /// <summary>Gets or sets the primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets the foreign key to the parent <see cref="RefSet"/>.</summary>
        public Guid RefSetId { get; set; }

        /// <summary>Gets or sets the foreign key to the associated <see cref="RefTerm"/>.</summary>
        public Guid RefTermId { get; set; }

        /// <summary>Gets or sets the navigation property to the parent <see cref="RefSet"/>.</summary>
        public RefSet RefSet { get; set; } = null!;

        /// <summary>Gets or sets the navigation property to the associated <see cref="RefTerm"/>.</summary>
        public RefTerm RefTerm { get; set; } = null!;
    }
}