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
    public int BrandId { get; set; }

    [Required(ErrorMessage = "Brand name is required.")]
    [StringLength(100, ErrorMessage = "Brand name cannot exceed 100 characters.")]
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
