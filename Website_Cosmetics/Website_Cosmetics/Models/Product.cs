using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

[Table("Product")]
public partial class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? Slug { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal BasePrice { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Embedding vector for RAG (stored as JSON string)
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Embedding { get; set; }

    public string? Ingredients { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? Rating { get; set; }

    public int ReviewCount { get; set; } = 0;

    public int LikeCount { get; set; } = 0;

    [StringLength(50)]
    public string? SPF { get; set; }

    [StringLength(50)]
    public string? Size { get; set; }

    [StringLength(50)]
    public string? Finish { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; }

    public int? BrandId { get; set; }

    public int? CategoryId { get; set; }

    [ForeignKey("BrandId")]
    [InverseProperty("Products")]
    public virtual Brand? Brand { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    [InverseProperty("Product")]
    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    [InverseProperty("Product")]
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    [InverseProperty("Product")]
    public virtual ICollection<ProductLike> ProductLikes { get; set; } = new List<ProductLike>();
}
