using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Website_Cosmetics.Models
{
    [Table("UserAddress")]
    public partial class UserAddress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AddressId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string AddressLine1 { get; set; } = null!;

        [StringLength(200)]
        public string? AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = null!;

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string Country { get; set; } = "United States";

        public bool IsDefault { get; set; } = false;

        [Column(TypeName = "datetime2(7)")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "datetime2(7)")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        [InverseProperty("UserAddresses")]
        public virtual User User { get; set; } = null!;

        // Helper property to get full address
        [NotMapped]
        public string FullAddress => $"{AddressLine1}{(string.IsNullOrEmpty(AddressLine2) ? "" : ", " + AddressLine2)}, {City}{(string.IsNullOrEmpty(State) ? "" : ", " + State)} {PostalCode}, {Country}".Trim();
    }
}

