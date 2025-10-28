using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

[Table("Brand")]
public partial class Brand
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid BrandId { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string? Country { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; }

    [InverseProperty("Brand")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
