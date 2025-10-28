using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Cosmetics.Models
{
    public class UserRole
    {
        [Key]
        public Guid UID { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserUID { get; set; }

        [Required]
        public Guid RoleUID { get; set; }

        [Column(TypeName = "datetime2(7)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2(7)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserUID")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("RoleUID")]
        public virtual Role Role { get; set; } = null!;
    }
}
