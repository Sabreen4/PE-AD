using Shared.Common.Entities;

namespace IdentityService.Domain.Entities
{
    /// <summary>
    /// Represents a system user belonging to an organization with an assigned role.
    /// </summary>
    public sealed class User : BaseEntity
    {
        /// <summary>Gets or sets the primary key.</summary>
        public Guid Id { get; set; }

      
        /// <summary>Gets or sets the associated organization identifier.</summary>
        public long OrganizationId { get; set; }

        /// <summary>Gets or sets the associated role identifier.</summary>
        public Guid RoleId { get; set; }

        /// <summary>Gets or sets the first name.</summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>Gets or sets the last name.</summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>Gets or sets the email address.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Gets or sets the phone number.</summary>
        public string Phone { get; set; } = string.Empty;

       

        /// <summary>Gets or sets the optional gender identifier.</summary>
        public Guid? GenderId { get; set; }

        /// <summary>Gets or sets the navigation property to the associated organization.</summary>
        public Organization Organization { get; set; } = null!;

        /// <summary>Gets or sets the navigation property to the associated role.</summary>
        public Role Role { get; set; } = null!;

        /// <summary>Gets or sets the collection of refresh tokens belonging to this user.</summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        /// <summary>Gets or sets the collection of OTP verification records belonging to this user.</summary>
        public ICollection<VerifyOtp> VerifyOtps { get; set; } = new List<VerifyOtp>();
    }
}