using Microsoft.EntityFrameworkCore.Storage; // For IDbContextTransaction
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SynopsisSI.Services.ListingService.Application.Interfaces.Persistence;

// This IUnitOfWork is specific to the ListingService's data context.
public interface IUnitOfWork : IAsyncDisposable
{
    IListingRepository Listings { get; } // Expose repositories specific to this UoW

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}