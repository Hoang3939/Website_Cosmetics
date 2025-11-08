using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

[Table("ProductVariant")]
public partial class ProductVariant
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int VariantId { get; set; }

    public int ProductId { get; set; }

    // Thông tin biến thể
    [Required]
    [StringLength(200)]
    public string VariantName { get; set; } = null!;

    // Màu sắc (cho son, phấn, nail polish, etc.)
    [StringLength(100)]
    public string? ColorName { get; set; }

    [StringLength(50)]
    public string? ColorCode { get; set; }

    [StringLength(50)]
    public string? ColorFamily { get; set; }

    // Kích thước/Dung tích (cho chai xịt, kem, serum, etc.)
    [StringLength(100)]
    public string? Size { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? SizeValue { get; set; }

    [StringLength(20)]
    public string? SizeUnit { get; set; }

    // Loại/Type (cho sản phẩm skincare)
    [StringLength(100)]
    public string? VariantType { get; set; }

    // Thông tin bán hàng
    [StringLength(100)]
    public string? SKU { get; set; }

    [StringLength(100)]
    public string? Barcode { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? Price { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal? CompareAtPrice { get; set; }

    public int Stock { get; set; } = 0;

    public int LowStockThreshold { get; set; } = 10;

    // Trạng thái
    public bool? IsActive { get; set; }

    public bool IsDefault { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("ProductId")]
    [InverseProperty("ProductVariants")]
    public virtual Product Product { get; set; } = null!;

    [InverseProperty("ProductVariant")]
    public virtual ICollection<ProductVariantImage> ProductVariantImages { get; set; } = new List<ProductVariantImage>();

    [InverseProperty("ProductVariant")]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [InverseProperty("ProductVariant")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // Helper property to get final price
    [NotMapped]
    public decimal FinalPrice => Price ?? Product?.BasePrice ?? 0;

    // Helper property to check if low stock
    [NotMapped]
    public bool IsLowStock => Stock < LowStockThreshold;
}

