using System;
using System.Collections.Generic;

namespace SynopsisSI.Shared.Events;

// Published by OrderService when an order is successfully placed.
// Consumed by ListingService (to update stock/status), NotificationService, etc.
public class OrderPlacedEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string OrderId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public List<OrderItemDetail> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    // Add others.... relevant details needed by consuming services
}

public class OrderItemDetail
{
    public string ListingId { get; set; } = string.Empty;
    public string ProductTitleSnapshot { get; set; } = string.Empty; // Important to snapshot
    public int Quantity { get; set; }
    public decimal PricePerItem { get; set; }
}