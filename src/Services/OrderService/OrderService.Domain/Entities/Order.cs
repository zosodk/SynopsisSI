using System;
using System.Collections.Generic;
using System.Linq;
using SynopsisSI.Services.OrderService.Domain.ValueObjects;

    namespace SynopsisSI.Services.OrderService.Domain.Entities;

    public class Order
    {
        public string Id { get; private set; }
        public string BuyerId { get; private set; }
        private readonly List<OrderItem> _orderItems = new();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
        public decimal TotalAmount { get; private set; }
        public string Currency { get; private set; }
        public OrderStatus Status { get; private set; }
        public Address ShippingAddress { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string? PaymentTransactionId { get; private set; }
        public int Version { get; private set; }

        private Order()
        {
            Id = Guid.NewGuid().ToString();
            BuyerId = string.Empty;
            ShippingAddress = Address.Create("N/A", "N/A", "N/A", "N/A"); 
            Currency = "USD";
            Status = OrderStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            Version = 1;
        }

        public static Order Create(string buyerId, Address shippingAddress, List<OrderItemProvisionalData> provisionalItems, string currency = "USD")
        {
            if (string.IsNullOrWhiteSpace(buyerId)) throw new ArgumentNullException(nameof(buyerId));
            if (shippingAddress == null) throw new ArgumentNullException(nameof(shippingAddress));
            if (provisionalItems == null || !provisionalItems.Any()) throw new ArgumentException("Order must have items.", nameof(provisionalItems));

            var order = new Order
            {
                BuyerId = buyerId,
                ShippingAddress = shippingAddress,
                Currency = currency,
                UpdatedAt = DateTime.UtcNow
            };
            foreach (var itemData in provisionalItems)
            {
                order._orderItems.Add(OrderItem.Create(itemData.ListingId, itemData.ProductTitleSnapshot, itemData.PriceAtPurchase, itemData.Quantity));
            }
            order.CalculateTotalAmount();
            return order;
        }
        
        public void SetPaymentDetails(string transactionId, bool paymentSuccessful)
        {
            PaymentTransactionId = transactionId;
            Status = paymentSuccessful ? OrderStatus.Paid : OrderStatus.PaymentFailed;
            UpdatedAt = DateTime.UtcNow;
            Version++;
        }

        public void MarkAsShipped(string? trackingNumber = null)
        {
            if (Status != OrderStatus.Paid && Status != OrderStatus.AwaitingShipment)
                throw new InvalidOperationException($"Order must be Paid or AwaitingShipment. Current: {Status}");
            Status = OrderStatus.Shipped;
            UpdatedAt = DateTime.UtcNow;
            Version++;
        }

        public void MarkAsDelivered()
        {
            if (Status != OrderStatus.Shipped)
                throw new InvalidOperationException($"Order must be Shipped. Current: {Status}");
            Status = OrderStatus.Delivered;
            UpdatedAt = DateTime.UtcNow;
            Version++;
        }

        public void CancelOrder(string reason = "Order cancelled by user/system.")
        {
            if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered || Status == OrderStatus.Completed)
                throw new InvalidOperationException($"Cannot cancel order in status {Status}.");
            Status = OrderStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
            Version++;
        }

        private void CalculateTotalAmount() => TotalAmount = _orderItems.Sum(item => item.PriceAtPurchase * item.Quantity);
    }

    public record OrderItemProvisionalData(string ListingId, string ProductTitleSnapshot, decimal PriceAtPurchase, int Quantity);
