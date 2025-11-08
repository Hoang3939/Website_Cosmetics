using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

[Table("CartItem")]
public partial class CartItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CartItemId { get; set; }

    public int CartId { get; set; }

    public int VariantId { get; set; }

    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Price { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("CartId")]
    [InverseProperty("CartItems")]
    public virtual ShoppingCart ShoppingCart { get; set; } = null!;

    [ForeignKey("VariantId")]
    [InverseProperty("CartItems")]
    public virtual ProductVariant ProductVariant { get; set; } = null!;

    // Helper property
    [NotMapped]
    public decimal Subtotal => Price * Quantity;
}

