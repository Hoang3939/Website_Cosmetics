using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

[Table("ProductImage")]
public partial class ProductImage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductImageId { get; set; }

    public int ProductId { get; set; }

    [Required]
    [StringLength(500)]
    public string Url { get; set; } = null!;

    [StringLength(255)]
    public string? AltText { get; set; }

    [StringLength(50)]
    public string ImageType { get; set; } = "gallery"; // 'cover', 'gallery', 'demo', 'video'

    public int DisplayOrder { get; set; } = 0;

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("ProductImages")]
    public virtual Product Product { get; set; } = null!;
}
