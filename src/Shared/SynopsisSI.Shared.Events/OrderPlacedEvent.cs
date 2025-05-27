using System;
using System.Collections.Generic;

namespace SynopsisSI.Shared.Events;

public class OrderPlacedEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string OrderId { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public List<OrderItemDetail> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } 
}

public class OrderItemDetail
{
    public string ListingId { get; set; } = string.Empty;
    public string ProductTitleSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal PricePerItem { get; set; }
}