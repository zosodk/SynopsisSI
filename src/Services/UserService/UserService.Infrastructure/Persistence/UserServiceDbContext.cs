using Microsoft.EntityFrameworkCore;
using SynopsisSI.Services.UserService.Domain.Entities;
// HUSK! ValueObjects namespace Hvis AddressVO 
using SynopsisSI.Services.UserService.Domain.ValueObjects;
using System; 
using System.Collections.Generic; 
using System.Linq; 

    namespace SynopsisSI.Services.UserService.Infrastructure.Persistence;

    public class UserServiceDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;

        public UserServiceDbContext(DbContextOptions<UserServiceDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("identity");

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                //OBS!
                // For PostgreSQL with Guid PKs, could use:
                // entity.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
                // If Id is string Guid, it's set in domain or here if not DB generated.

                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.HasIndex(u => u.Username).IsUnique();

                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.ProfileImageUrl).HasMaxLength(500);

                entity.OwnsOne(u => u.PrimaryAddress, addr =>
                {
                    addr.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
                    addr.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(100);
                    addr.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
                    addr.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(100);
                });

                entity.Property(u => u.Roles)
                      .HasConversion(
                          v => string.Join(',', v), // Convert List<string> -> comma-separated string
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() // Convert back to List<string>
                      )
                      //  change tracking of collections in EF Core with relational DBs
                      .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                          (c1, c2) => c1!.SequenceEqual(c2!),
                          c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                          c => c.ToList()));
                
                entity.Property(u => u.IsActive).IsRequired();
                entity.Property(u => u.CreatedAt).IsRequired();
                entity.Property(u => u.UpdatedAt);
            });
        }
    }