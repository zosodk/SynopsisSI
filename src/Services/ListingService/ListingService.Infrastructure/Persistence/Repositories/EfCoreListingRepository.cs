    using Microsoft.EntityFrameworkCore;
    using SynopsisSI.Services.ListingService.Application.Interfaces.Persistence;
    using SynopsisSI.Services.ListingService.Domain.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    namespace SynopsisSI.Services.ListingService.Infrastructure.Persistence.Repositories;

    public class EfCoreListingRepository : IListingRepository
    {
        private readonly ListingServiceDbContext _context;

        public EfCoreListingRepository(ListingServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ListingItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _context.Listings.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task AddAsync(ListingItem listing, CancellationToken cancellationToken = default)
        {
            if (listing == null) throw new ArgumentNullException(nameof(listing));
            await _context.Listings.AddAsync(listing, cancellationToken);
        }

        public Task UpdateAsync(ListingItem listing, CancellationToken cancellationToken = default)
        {
            if (listing == null) throw new ArgumentNullException(nameof(listing));
            _context.Entry(listing).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ListingItem listing, CancellationToken cancellationToken = default)
        {
            if (listing == null) throw new ArgumentNullException(nameof(listing));
            _context.Listings.Remove(listing);
            return Task.CompletedTask;
        }

        public async Task<List<ListingItem>> FindAsync(Expression<Func<ListingItem, bool>> predicate, int skip, int limit, CancellationToken cancellationToken = default)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), "Skip cannot be negative.");
            if (limit <= 0) throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be positive.");
            return await _context.Listings.Where(predicate).Skip(skip).Take(limit).ToListAsync(cancellationToken);
        }

        public async Task<List<ListingItem>> GetBySellerIdAsync(string sellerId, int skip, int limit, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sellerId)) return new List<ListingItem>();
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), "Skip cannot be negative.");
            if (limit <= 0) throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be positive.");
            return await _context.Listings.Where(l => l.SellerId == sellerId)
                                 .OrderByDescending(l => l.CreatedAt)
                                 .Skip(skip).Take(limit).ToListAsync(cancellationToken);
        }
    }