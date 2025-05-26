using SynopsisSI.Services.OrderService.Application.Interfaces.Persistence;
using SynopsisSI.Services.OrderService.Application.Interfaces.MessageBus;
using SynopsisSI.Services.OrderService.Domain.Entities;
using SynopsisSI.Services.OrderService.Domain.ValueObjects;
using SynopsisSI.Shared.Events; 
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace SynopsisSI.Services.OrderService.Application.Features.Orders.Commands.PlaceOrder;

        public class PlaceOrderCommandHandler
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMessageBusPublisher _messageBusPublisher;
            private readonly ILogger<PlaceOrderCommandHandler> _logger;

            public PlaceOrderCommandHandler(
                IUnitOfWork unitOfWork,
                IMessageBusPublisher messageBusPublisher,
                ILogger<PlaceOrderCommandHandler> logger)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _messageBusPublisher = messageBusPublisher ?? throw new ArgumentNullException(nameof(messageBusPublisher));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<string> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Handling PlaceOrderCommand for BuyerId: {BuyerId}", request.BuyerId);
                
                var shippingAddress = Address.Create(request.ShippingAddress.Street, request.ShippingAddress.City, request.ShippingAddress.PostalCode, request.ShippingAddress.Country);
                
                var provisionalItems = request.Items.Select(i =>
                    new OrderItemProvisionalData(i.ListingId, i.ProductTitleSnapshot, i.PriceAtPurchase, i.Quantity)
                ).ToList();

                var order = Domain.Entities.Order.Create(request.BuyerId, shippingAddress, provisionalItems, request.Currency);

                IDbContextTransaction? transaction = null;
                try
                {
                    transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    
                    await _unitOfWork.Orders.AddAsync(order, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var orderPlacedEvent = new OrderPlacedEvent
                    {
                        OrderId = order.Id,
                        BuyerId = order.BuyerId,
                        Items = order.OrderItems.Select(oi => new OrderItemDetail
                        {
                            ListingId = oi.ListingId,
                            ProductTitleSnapshot = oi.ProductTitleSnapshot,
                            Quantity = oi.Quantity,
                            PricePerItem = oi.PriceAtPurchase
                        }).ToList(),
                        TotalAmount = order.TotalAmount,
                        Currency = order.Currency,
                        OrderDate = order.CreatedAt
                    };
                    await _messageBusPublisher.PublishAsync(orderPlacedEvent, cancellationToken);
                    
                    await _unitOfWork.CommitTransactionAsync(transaction, cancellationToken);
                    _logger.LogInformation("Order {OrderId} placed successfully and OrderPlacedEvent published.", order.Id);
                    return order.Id;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error placing order for BuyerId {BuyerId}. Rolling back transaction.", request.BuyerId);
                    if(transaction != null) await _unitOfWork.RollbackTransactionAsync(transaction, CancellationToken.None);
                    throw;
                }
            }
        }
