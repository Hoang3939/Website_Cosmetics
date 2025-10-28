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
    public Guid ProductId { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    public string? Slug { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? Ingredients { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; }

    public Guid? BrandId { get; set; }

    public Guid? CategoryId { get; set; }

    [ForeignKey("BrandId")]
    [InverseProperty("Products")]
    public virtual Brand? Brand { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
