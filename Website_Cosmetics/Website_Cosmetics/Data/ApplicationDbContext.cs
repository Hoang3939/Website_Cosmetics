using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Authentication & Authorization tables
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<EmailConfirmationToken> EmailConfirmationTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        // Product & Business tables
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductVariantImage> ProductVariantImages { get; set; }

        // Shopping & Orders tables
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // Review & Like tables
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ProductLike> ProductLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.HasIndex(e => e.RoleName).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure UserRole entity
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.UserRoleId);
                entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PasswordResetToken entity
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(e => e.TokenId);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PasswordResetTokens)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure EmailConfirmationToken entity
            modelBuilder.Entity<EmailConfirmationToken>(entity =>
            {
                entity.HasKey(e => e.TokenId);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.EmailConfirmationTokens)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.PermissionId);
                entity.HasIndex(e => e.PermissionName).IsUnique();
                entity.HasIndex(e => e.Category);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure RolePermission entity
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.RolePermissionId);
                entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(d => d.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserPermission entity
            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.HasKey(e => e.UserPermissionId);
                entity.HasIndex(e => new { e.UserId, e.PermissionId }).IsUnique();
                entity.HasIndex(e => e.GrantedBy);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserPermissions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.UserPermissions)
                    .HasForeignKey(d => d.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.GrantedByUser)
                    .WithMany(p => p.GrantedPermissions)
                    .HasForeignKey(d => d.GrantedBy)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure Brand entity
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasKey(e => e.BrandId);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
            });

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.BasePrice).HasColumnType("decimal(12, 2)");
                entity.Property(e => e.Rating).HasColumnType("decimal(3, 2)");

                entity.HasOne(d => d.Brand)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.BrandId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Product_Brand");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Product_Category");
            });

            // Configure ProductImage entity
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.ProductImageId);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.ImageType).HasDefaultValue("gallery");
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ProductImage_Product");
            });

            // Configure ProductVariant entity
            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.HasKey(e => e.VariantId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.ColorName);
                entity.HasIndex(e => e.Size);
                entity.HasIndex(e => e.SKU);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDefault).HasDefaultValue(false);
                entity.Property(e => e.Stock).HasDefaultValue(0);
                entity.Property(e => e.LowStockThreshold).HasDefaultValue(10);
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
                entity.Property(e => e.CompareAtPrice).HasColumnType("decimal(12, 2)");
                entity.Property(e => e.SizeValue).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductVariants)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ProductVariant_Product");
            });

            // Configure ProductVariantImage entity
            modelBuilder.Entity<ProductVariantImage>(entity =>
            {
                entity.HasKey(e => e.ImageId);
                entity.HasIndex(e => e.VariantId);
                entity.Property(e => e.IsPrimary).HasDefaultValue(false);
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.ProductVariant)
                    .WithMany(p => p.ProductVariantImages)
                    .HasForeignKey(d => d.VariantId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ProductVariantImage_Variant");
            });

            // Configure ShoppingCart entity
            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.HasKey(e => e.CartId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.SessionId);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ShoppingCarts)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure CartItem entity
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.CartItemId);
                entity.HasIndex(e => e.CartId);
                entity.HasIndex(e => e.VariantId);
                entity.Property(e => e.Quantity).HasDefaultValue(1);
                entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.ShoppingCart)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(d => d.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.ProductVariant)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(d => d.VariantId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.Status).HasDefaultValue("Pending");
                entity.Property(e => e.PaymentStatus).HasDefaultValue("Pending");
                entity.Property(e => e.SubTotal).HasColumnType("decimal(12, 2)");
                entity.Property(e => e.ShippingFee).HasColumnType("decimal(12, 2)").HasDefaultValue(0);
                entity.Property(e => e.Discount).HasColumnType("decimal(12, 2)").HasDefaultValue(0);
                entity.Property(e => e.Total).HasColumnType("decimal(12, 2)");
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.OrderItemId);
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.VariantId);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(12, 2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(12, 2)");
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.ProductVariant)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.VariantId);
            });

            // Configure ProductReview entity
            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.HasKey(e => e.ReviewId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsVerifiedPurchase).HasDefaultValue(false);
                entity.Property(e => e.HelpfulCount).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductReviews)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ProductReviews)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ProductLike entity
            modelBuilder.Entity<ProductLike>(entity =>
            {
                entity.HasKey(e => e.LikeId);
                entity.HasIndex(e => new { e.ProductId, e.UserId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductLikes)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ProductLikes)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
