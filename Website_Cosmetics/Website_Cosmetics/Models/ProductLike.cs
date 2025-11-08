using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

[Table("ProductLike")]
public partial class ProductLike
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LikeId { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    [ForeignKey("ProductId")]
    [InverseProperty("ProductLikes")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ProductLikes")]
    public virtual User User { get; set; } = null!;
}

