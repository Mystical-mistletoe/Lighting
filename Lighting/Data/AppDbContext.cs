using Lighting.Models;
using Microsoft.EntityFrameworkCore;

namespace Lighting.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Characteristic> Characteristics { get; set; }
        public DbSet<CharacteristicValue> CharacteristicValues { get; set; }
        public DbSet<ProductCharacteristic> ProductCharacteristics { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=LightingDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Accounts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Login).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Password).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
                entity.HasIndex(e => e.Login).IsUnique();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Login).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.Login).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.ToTable("Manufacturers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Country).HasMaxLength(50);
                entity.Property(e => e.Website).HasMaxLength(200);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Article).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Article).IsUnique();
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.ManufacturerId);
                entity.HasIndex(e => e.Price);

                entity.HasOne(e => e.Category)
                    .WithMany(e => e.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Manufacturer)
                    .WithMany(e => e.Products)
                    .HasForeignKey(e => e.ManufacturerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Characteristic>(entity =>
            {
                entity.ToTable("Characteristics");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<CharacteristicValue>(entity =>
            {
                entity.ToTable("CharacteristicValues");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Value).HasMaxLength(500).IsRequired();
                entity.HasIndex(e => new { e.CharacteristicId, e.Value }).IsUnique();

                entity.HasOne(e => e.Characteristic)
                    .WithMany(e => e.CharacteristicValues)
                    .HasForeignKey(e => e.CharacteristicId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProductCharacteristic>(entity =>
            {
                entity.ToTable("ProductCharacteristics");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.CharacteristicId);
                entity.HasIndex(e => e.CharacteristicValueId);
                entity.HasIndex(e => new { e.ProductId, e.CharacteristicId }).IsUnique();

                entity.HasOne(e => e.Product)
                    .WithMany(e => e.ProductCharacteristics)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Characteristic)
                    .WithMany(e => e.ProductCharacteristics)
                    .HasForeignKey(e => e.CharacteristicId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CharacteristicValue)
                    .WithMany(e => e.ProductCharacteristics)
                    .HasForeignKey(e => e.CharacteristicValueId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
