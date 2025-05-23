    using Microsoft.EntityFrameworkCore.Storage;
    using SynopsisSI.Services.ListingService.Application.Interfaces.Persistence;
    using SynopsisSI.Services.ListingService.Infrastructure.Persistence.Repositories;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    namespace SynopsisSI.Services.ListingService.Infrastructure.Persistence.Common;

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ListingServiceDbContext _dbContext;
        private IListingRepository? _listingRepository;

        public UnitOfWork(ListingServiceDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public IListingRepository Listings => _listingRepository ??= new EfCoreListingRepository(_dbContext);

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