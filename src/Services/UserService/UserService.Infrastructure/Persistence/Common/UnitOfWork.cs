using Microsoft.EntityFrameworkCore.Storage;
using SynopsisSI.Services.UserService.Application.Interfaces.Persistence;
using SynopsisSI.Services.UserService.Infrastructure.Persistence.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SynopsisSI.Services.UserService.Infrastructure.Persistence.Common;

public class UnitOfWork : IUnitOfWork
{
    private readonly UserServiceDbContext _dbContext;
    private IUserRepository? _userRepository;

    public UnitOfWork(UserServiceDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IUserRepository Users => _userRepository ??= new EfCoreUserRepository(_dbContext);

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction == null) throw new ArgumentNullException(nameof(transaction));
        await transaction.RollbackAsync(cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}