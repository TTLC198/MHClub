using MHClub.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MHClub.Domain;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
        Database.EnsureCreated();
    }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Trace);
    }

    public DbSet<Role> Roles { get; set; }
    public DbSet<Tariff> Tariffs { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Condition> Conditions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Ad> Ads { get; set; }
    public DbSet<Favourite> Favourites { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Media> Media { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId);

        modelBuilder.Entity<Ad>()
            .HasOne(a => a.Category)
            .WithMany()
            .HasForeignKey(a => a.CategoryId);

        modelBuilder.Entity<Ad>()
            .HasOne(a => a.Tariff)
            .WithMany()
            .HasForeignKey(a => a.TariffId);
        
        modelBuilder.Entity<Ad>()
            .HasOne(a => a.Seller)
            .WithMany()
            .HasForeignKey(a => a.SellerId);

        modelBuilder.Entity<Ad>()
            .HasOne(a => a.Condition)
            .WithMany()
            .HasForeignKey(a => a.ConditionId);

        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.Ad)
            .WithMany()
            .HasForeignKey(f => f.AdId);

        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId);

        modelBuilder.Entity<Complaint>()
            .HasOne(c => c.Ad)
            .WithMany()
            .HasForeignKey(c => c.AdId);

        modelBuilder.Entity<Complaint>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId);
    }
}