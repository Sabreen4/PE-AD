using Shared.Common.Entities;

namespace MetaDataService.Domain.Entities
{
    /// <summary>
    /// Represents a named group/category of reference terms.
    /// </summary>
    public sealed class RefSet : BaseEntity
    {
        /// <summary>Gets or sets the primary key.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets the unique name (key) of the reference set.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the description of the reference set.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>Gets or sets the collection of junction entities linking this set to reference terms.</summary>
        public ICollection<SetRefTerm> SetRefTerms { get; set; } = new List<SetRefTerm>();
    }
}