using Shared.Common.Entities;

namespace MetaDataService.Domain.Entities
{
    /// <summary>
    /// Represents a reference term (lookup value) that can belong to one or more reference sets.
    /// </summary>
    public sealed class RefTerm : BaseEntity
    {
        /// <summary>Gets or sets the primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets the display name of the reference term.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the description of the reference term.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Gets or sets the collection of junction entities linking this term to reference sets.</summary>
        public ICollection<SetRefTerm> SetRefTerms { get; set; } = new List<SetRefTerm>();
    }
}