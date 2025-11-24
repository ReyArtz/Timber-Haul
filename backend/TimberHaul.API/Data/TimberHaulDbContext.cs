using Microsoft.EntityFrameworkCore;
using TimberHaul.API.Models;

namespace TimberHaul.API.Data;

public class TimberHaulDbContext : DbContext
{
    public TimberHaulDbContext(DbContextOptions<TimberHaulDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ForesterProfile> ForesterProfiles { get; set; }
    public DbSet<DeliveryProfile> DeliveryProfiles { get; set; }
    public DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public DbSet<ForestPlot> ForestPlots { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<WoodInventory> WoodInventories { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Load> Loads { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Equipment> Equipment { get; set; }
    public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure enum mappings for PostgreSQL
        modelBuilder.HasPostgresEnum<UserRole>();
        modelBuilder.HasPostgresEnum<LoadStatus>();
        modelBuilder.HasPostgresEnum<PaymentStatus>();
        modelBuilder.HasPostgresEnum<PaymentMethod>();
        modelBuilder.HasPostgresEnum<EquipmentType>();
        modelBuilder.HasPostgresEnum<WoodType>();

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.HasOne(u => u.ForesterProfile)
                .WithOne(f => f.User)
                .HasForeignKey<ForesterProfile>(f => f.ForesterId);

            entity.HasOne(u => u.DeliveryProfile)
                .WithOne(d => d.User)
                .HasForeignKey<DeliveryProfile>(d => d.DriverId);

            entity.HasOne(u => u.CustomerProfile)
                .WithOne(c => c.User)
                .HasForeignKey<CustomerProfile>(c => c.CustomerId);
        });

        // ForesterProfile Configuration
        modelBuilder.Entity<ForesterProfile>(entity =>
        {
            entity.HasMany(f => f.ForestPlots)
                .WithOne(p => p.Forester)
                .HasForeignKey(p => p.ForesterId);

            entity.HasMany(f => f.Products)
                .WithOne(p => p.Forester)
                .HasForeignKey(p => p.ForesterId);
        });

        // Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.ProductName);
            entity.HasIndex(e => e.WoodType);
            entity.HasIndex(e => e.IsAvailable);
        });

        // CartItem Configuration
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(e => new { e.CustomerId, e.ProductId }).IsUnique();
        });

        // Order Configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.OrderStatus);
        });

        // Load Configuration
        modelBuilder.Entity<Load>(entity =>
        {
            entity.HasIndex(e => e.LoadNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PaymentStatus);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DueDate);
        });

        // Review Configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.Load)
                .WithMany(l => l.Reviews)
                .HasForeignKey(r => r.LoadId);

            entity.HasOne(r => r.Customer)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CustomerId);

            entity.HasOne(r => r.Driver)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Equipment Configuration
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.Equipment)
                .HasForeignKey(e => e.OwnerId);

            entity.HasMany(e => e.MaintenanceLogs)
                .WithOne(m => m.Equipment)
                .HasForeignKey(m => m.EquipmentId);
        });

        // Configure decimal precision
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetPrecision(10);
                    property.SetScale(2);
                }
            }
        }
    }
}