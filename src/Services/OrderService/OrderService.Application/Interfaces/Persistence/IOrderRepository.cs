using SynopsisSI.Services.OrderService.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SynopsisSI.Services.OrderService.Application.Interfaces.Persistence;
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default); 
    Task<List<Order>> GetOrdersByBuyerIdAsync(string buyerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}