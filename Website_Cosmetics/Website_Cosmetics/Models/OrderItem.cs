using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

[Table("OrderItem")]
public partial class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int VariantId { get; set; }

    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string ColorName { get; set; } = null!;

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey("OrderId")]
    [InverseProperty("OrderItems")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("VariantId")]
    [InverseProperty("OrderItems")]
    public virtual ProductVariant ProductVariant { get; set; } = null!;
}

