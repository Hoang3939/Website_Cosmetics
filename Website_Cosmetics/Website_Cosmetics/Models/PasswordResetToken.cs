using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Cosmetics.Models
{
    public class PasswordResetToken
    {
        [Key]
        public Guid UID { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserUID { get; set; }

        [Required]
        [StringLength(255)]
        public string Token { get; set; } = string.Empty;

        [Column(TypeName = "datetime2(7)")]
        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        [Column(TypeName = "datetime2(7)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2(7)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserUID")]
        public virtual User User { get; set; } = null!;
    }
}
