using Microsoft.EntityFrameworkCore;
using SynopsisSI.Services.ListingService.Domain.Entities;
using MongoDB.EntityFrameworkCore.Extensions;
using System;

    namespace SynopsisSI.Services.ListingService.Infrastructure.Persistence;

    public class ListingServiceDbContext : DbContext
    {
        public DbSet<ListingItem> Listings { get; init; }

        public ListingServiceDbContext(DbContextOptions<ListingServiceDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ListingItem>().ToCollection("Listings");

            modelBuilder.Entity<ListingItem>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.SellerId).IsRequired();
                entity.Property(l => l.Title).IsRequired().HasMaxLength(150);
                entity.Property(l => l.Description).HasMaxLength(10000);
                entity.Property(l => l.Category).IsRequired();
                entity.Property(l => l.Price).HasPrecision(18, 2);
                entity.Property(l => l.Currency).IsRequired().HasMaxLength(3);
                entity.Property(l => l.Condition).IsRequired();

                entity.OwnsOne(l => l.Location, loc =>
                {
                    loc.Property(g => g.Type).HasElementName("type").IsRequired();
                    loc.Property(g => g.Longitude).HasElementName("longitude");
                    loc.Property(g => g.Latitude).HasElementName("latitude");
                });

                entity.Property(l => l.Status)
                    .HasConversion(s => s.ToString(), s => (ListingStatus)Enum.Parse(typeof(ListingStatus), s))
                    .IsRequired();

                entity.Property(l => l.Version).IsConcurrencyToken();

                entity.HasIndex(l => l.SellerId);
                entity.HasIndex(l => l.Category);
                entity.HasIndex(l => l.Status);
                entity.HasIndex(l => l.Price);
                entity.HasIndex(l => new { l.Category, l.Status });
            });
        }
    }