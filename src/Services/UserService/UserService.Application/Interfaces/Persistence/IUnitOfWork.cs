using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SynopsisSI.Services.UserService.Application.Interfaces.Persistence;

public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    Task CommitTransactionAsync(IDbContextTransaction transaction, 
        CancellationToken cancellationToken = default);
    
    Task RollbackTransactionAsync(IDbContextTransaction transaction, 
        CancellationToken cancellationToken = default);
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}