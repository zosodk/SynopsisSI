using Microsoft.EntityFrameworkCore.Storage;
using SynopsisSI.Services.OrderService.Application.Interfaces.Persistence;
using SynopsisSI.Services.OrderService.Infrastructure.Persistence.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SynopsisSI.Services.OrderService.Infrastructure.Persistence.Common;
public class UnitOfWork : IUnitOfWork
{
    private readonly OrderServiceDbContext _dbContext;
    private IOrderRepository? _orderRepository;
    public UnitOfWork(OrderServiceDbContext dbContext) => _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    public IOrderRepository Orders => _orderRepository ??= new EfCoreOrderRepository(_dbContext);
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Database.BeginTransactionAsync(cancellationToken);
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
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.SaveChangesAsync(cancellationToken);
    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}