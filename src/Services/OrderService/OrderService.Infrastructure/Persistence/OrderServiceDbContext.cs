using Microsoft.EntityFrameworkCore;
using SynopsisSI.Services.OrderService.Domain.Entities;
using SynopsisSI.Services.OrderService.Domain.ValueObjects; // For Address
using MongoDB.EntityFrameworkCore.Extensions;
using System;

    namespace SynopsisSI.Services.OrderService.Infrastructure.Persistence;

    public class OrderServiceDbContext : DbContext
    {
        public DbSet<Order> Orders { get; init; }

        public OrderServiceDbContext(DbContextOptions<OrderServiceDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().ToCollection("Orders");

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.BuyerId).IsRequired();
                entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
                entity.Property(o => o.Currency).IsRequired().HasMaxLength(3);
                
                entity.Property(o => o.Status)
                    .HasConversion(s => s.ToString(), s => (OrderStatus)Enum.Parse(typeof(OrderStatus), s))
                    .IsRequired();

                entity.OwnsOne(o => o.ShippingAddress, sa =>
                {
                    sa.Property(a => a.Street).HasElementName("street").IsRequired();
                    sa.Property(a => a.City).HasElementName("city").IsRequired();
                    sa.Property(a => a.PostalCode).HasElementName("postalCode").IsRequired();
                    sa.Property(a => a.Country).HasElementName("country").IsRequired();
                });

                // OrderItem is an owned collection (embedded array)
                entity.OwnsMany(o => o.OrderItems, oi =>
                {
                    oi.Property(i => i.ListingId).IsRequired();
                    oi.Property(i => i.ProductTitleSnapshot).IsRequired();
                    oi.Property(i => i.PriceAtPurchase).HasPrecision(18, 2);
                    oi.Property(i => i.Quantity);
                    // oi.WithOwner().HasForeignKey("OrderId"); // Not needed for MongoDB embedded
                });
                
                entity.Property(o => o.Version).IsConcurrencyToken();
                entity.HasIndex(o => o.BuyerId);
                entity.HasIndex(o => o.Status);
            });
        }
    }