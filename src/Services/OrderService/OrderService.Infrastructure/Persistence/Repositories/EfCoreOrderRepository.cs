    using Microsoft.EntityFrameworkCore;
    using SynopsisSI.Services.OrderService.Application.Interfaces.Persistence;
    using SynopsisSI.Services.OrderService.Domain.Entities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace SynopsisSI.Services.OrderService.Infrastructure.Persistence.Repositories;

    public class EfCoreOrderRepository : IOrderRepository
    {
        private readonly OrderServiceDbContext _context;
        public EfCoreOrderRepository(OrderServiceDbContext context) => _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
            string.IsNullOrWhiteSpace(id) ? null : await _context.Orders.FindAsync(new object[] { id }, cancellationToken);

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            await _context.Orders.AddAsync(order, cancellationToken);
        }

        public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            _context.Entry(order).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public async Task<List<Order>> GetOrdersByBuyerIdAsync(string buyerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(buyerId)) return new List<Order>();
            return await _context.Orders.Where(o => o.BuyerId == buyerId)
                                 .OrderByDescending(o => o.CreatedAt)
                                 .Skip((pageNumber - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync(cancellationToken);
        }
    }