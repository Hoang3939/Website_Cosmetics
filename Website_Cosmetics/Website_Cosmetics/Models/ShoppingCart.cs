using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

[Table("ShoppingCart")]
public partial class ShoppingCart
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CartId { get; set; }

    public int? UserId { get; set; }

    [StringLength(255)]
    public string? SessionId { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    [InverseProperty("ShoppingCarts")]
    public virtual User? User { get; set; }

    [InverseProperty("ShoppingCart")]
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    // Helper property to calculate total
    [NotMapped]
    public decimal Total => CartItems?.Sum(item => item.Subtotal) ?? 0;

    [NotMapped]
    public int TotalItems => CartItems?.Sum(item => item.Quantity) ?? 0;
}

