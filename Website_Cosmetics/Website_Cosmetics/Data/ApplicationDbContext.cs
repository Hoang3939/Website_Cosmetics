using Microsoft.EntityFrameworkCore;
using Website_Cosmetics.Models;

namespace Website_Cosmetics.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<EmailConfirmationToken> EmailConfirmationTokens { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UID);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.UID);
                entity.HasIndex(e => e.RoleName).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure UserRole entity
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.UID);
                entity.HasIndex(e => new { e.UserUID, e.RoleUID }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserUID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleUID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PasswordResetToken entity
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(e => e.UID);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserUID);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PasswordResetTokens)
                    .HasForeignKey(d => d.UserUID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure EmailConfirmationToken entity
            modelBuilder.Entity<EmailConfirmationToken>(entity =>
            {
                entity.HasKey(e => e.UID);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserUID);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.EmailConfirmationTokens)
                    .HasForeignKey(d => d.UserUID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Permission entity
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.UID);
                entity.HasIndex(e => e.PermissionName).IsUnique();
                entity.HasIndex(e => e.Category);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure RolePermission entity
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.UID);
                entity.HasIndex(e => new { e.RoleUID, e.PermissionUID }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(d => d.RoleUID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(d => d.PermissionUID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserPermission entity
            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.HasKey(e => e.UID);
                entity.HasIndex(e => new { e.UserUID, e.PermissionUID }).IsUnique();
                entity.HasIndex(e => e.GrantedBy);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserPermissions)
                    .HasForeignKey(d => d.UserUID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Permission)
                    .WithMany(p => p.UserPermissions)
                    .HasForeignKey(d => d.PermissionUID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.GrantedByUser)
                    .WithMany(p => p.GrantedPermissions)
                    .HasForeignKey(d => d.GrantedBy)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}
