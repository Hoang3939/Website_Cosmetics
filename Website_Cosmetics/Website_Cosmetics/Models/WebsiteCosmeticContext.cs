using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Website_Cosmetics.Models;

public partial class WebsiteCosmeticContext : DbContext
{
    public WebsiteCosmeticContext()
    {
    }

    public WebsiteCosmeticContext(DbContextOptions<WebsiteCosmeticContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId);
            
            entity.Property(e => e.BrandId).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            
            entity.Property(e => e.CategoryId).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);
            
            entity.Property(e => e.ProductId).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Product_Brand");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Product_Category");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ProductImageId);
            
            entity.Property(e => e.ProductImageId).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.IsCover).HasDefaultValue(false);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ProductImage_Product");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
