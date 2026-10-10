using Shared.Common.Entities;

namespace IdentityService.Domain.Entities
{
    /// <summary>
    /// Represents a refresh token issued to a user for maintaining an authenticated session.
    /// </summary>
    public sealed class RefreshToken : BaseEntity
    {
        /// <summary>Gets or sets the primary key.</summary>
        public long Id { get; set; }

        /// <summary>Gets or sets the associated user identifier.</summary>
        public Guid UserId { get; set; }

        /// <summary>Gets or sets the refresh token value.</summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>Gets or sets the expiration date and time of the refresh token.</summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>Gets or sets whether the refresh token has been revoked.</summary>
        public bool IsRevoked { get; set; }

        /// <summary>Gets or sets the navigation property to the associated user.</summary>
        public User User { get; set; } = null!;
    }
}