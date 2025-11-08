using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

[Table("Orders")]
public partial class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string OrderNumber { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled

    [Column(TypeName = "decimal(12, 2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal ShippingFee { get; set; } = 0;

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Discount { get; set; } = 0;

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Total { get; set; }

    [StringLength(500)]
    public string? ShippingAddress { get; set; }

    [StringLength(100)]
    public string? ShippingMethod { get; set; }

    [StringLength(100)]
    public string? PaymentMethod { get; set; }

    [StringLength(50)]
    public string PaymentStatus { get; set; } = "Pending";

    public string? Notes { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    [InverseProperty("Orders")]
    public virtual User? User { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

