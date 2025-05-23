using System;

namespace SynopsisSI.Shared.Events;

// Published by ListingService when a listing's status changes -becomes Sold, Delisted).
// Could be consumed by an othersubservice to update stock/status, NotificationService, etc.
public class ListingStatusChangedEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string ListingId { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? OrderId { get; set; } //  status change is due to an order
}