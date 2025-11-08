using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Website_Cosmetics.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First name can only contain letters and spaces")]
        public string? FirstName { get; set; }

        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Last name can only contain letters and spaces")]
        public string? LastName { get; set; }

        [StringLength(11, MinimumLength = 10)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Phone number can only contain digits")]
        public string? PhoneNumber { get; set; }

        public bool IsEmailConfirmed { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "datetime2(7)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2(7)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
        public virtual ICollection<EmailConfirmationToken> EmailConfirmationTokens { get; set; } = new List<EmailConfirmationToken>();
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
        public virtual ICollection<UserPermission> GrantedPermissions { get; set; } = new List<UserPermission>();
        
        // Shopping & Orders
        [InverseProperty("User")]
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
        
        [InverseProperty("User")]
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        
        // Reviews & Likes
        [InverseProperty("User")]
        public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
        
        [InverseProperty("User")]
        public virtual ICollection<ProductLike> ProductLikes { get; set; } = new List<ProductLike>();

        // Computed properties
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
