using Shared.Common.Entities;

namespace IdentityService.Domain.Entities
{
    /// <summary>
    /// Represents an OTP-based forgot-password request for a user.
    /// </summary>
public sealed class VerifyOtp : BaseEntity
    {
        /// <summary>Gets or sets the primary key.</summary>
        public long Id { get; set; }

        
        /// <summary>Gets or sets the associated user identifier.</summary>
        public Guid UserId { get; set; }

        /// <summary>Gets or sets the one-time password.</summary>
        public string Otp { get; set; } = string.Empty;

        /// <summary>Gets or sets the expiration date and time of the OTP.</summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>Gets or sets whether the OTP has been verified.</summary>
        public bool IsVerified { get; set; }

      
        /// <summary>Gets or sets the navigation property to the associated user.</summary>
        public User User { get; set; } = null!;
    }
}