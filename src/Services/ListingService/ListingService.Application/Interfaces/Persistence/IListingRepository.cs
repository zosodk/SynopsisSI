// SynopsisSI/src/Services/ListingService/ListingService.Application/Interfaces/Persistence/IListingRepository.cs
using SynopsisSI.Services.ListingService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;

namespace SynopsisSI.Services.ListingService.Application.Interfaces.Persistence;

public interface IListingRepository
{
    Task<ListingItem?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(ListingItem listing, CancellationToken cancellationToken = default);
    Task UpdateAsync(ListingItem listing, CancellationToken cancellationToken = default); // EF Core tracks, UnitoW saves
    Task DeleteAsync(ListingItem listing, CancellationToken cancellationToken = default); // EF Core tracks, UoW saves
    Task<List<ListingItem>> FindAsync(Expression<Func<ListingItem, bool>> predicate, int skip, int limit, CancellationToken cancellationToken = default);
    Task<List<ListingItem>> GetBySellerIdAsync(string sellerId, int skip, int limit, CancellationToken cancellationToken = default);
    // Rememver to add other query methods as needed, dvs., search, filter by category/status
}