using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Cosmetics.Models
{
    public class RolePermission
    {
        [Key]
        public Guid UID { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RoleUID { get; set; }

        [Required]
        public Guid PermissionUID { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2(7)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("RoleUID")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("PermissionUID")]
        public virtual Permission Permission { get; set; } = null!;
    }
}
