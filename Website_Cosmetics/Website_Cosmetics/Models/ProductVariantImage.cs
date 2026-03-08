using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

[Table("ProductVariantImage")]
public partial class ProductVariantImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ImageId { get; set; }

    public int VariantId { get; set; }

    [Required]
    [StringLength(500)]
    public string Url { get; set; } = null!;

    [StringLength(255)]
    public string? AltText { get; set; }

    public bool IsPrimary { get; set; } = false;

    public bool IsMakeupReference { get; set; } = false; // Ảnh dùng cho Virtual Try-On (có khuôn mặt người mẫu)

    public int DisplayOrder { get; set; } = 0;

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey("VariantId")]
    [InverseProperty("ProductVariantImages")]
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}

