using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

[Table("Category")]
public partial class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid CategoryId { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
