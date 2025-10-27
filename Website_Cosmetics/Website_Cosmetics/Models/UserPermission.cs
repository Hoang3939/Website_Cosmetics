using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Cosmetics.Models
{
    public class UserPermission
    {
        [Key]
        public Guid UID { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserUID { get; set; }

        [Required]
        public Guid PermissionUID { get; set; }

        public Guid? GrantedBy { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2(7)")]
        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "datetime2(7)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2(7)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserUID")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PermissionUID")]
        public virtual Permission Permission { get; set; } = null!;

        [ForeignKey("GrantedBy")]
        public virtual User? GrantedByUser { get; set; }
    }
}
