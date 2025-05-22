
// using SynopsisSI.Shared.Domain.Common;
// If using a shared MediatR INotification or base event

namespace SynopsisSI.Services.ListingService.Domain.Events;

// public class ListingCreatedDomainEvent : INotification // ssssif using MediatR for in-process domain events
public class ListingCreatedDomainEvent
{
    public string ListingId { get; }
    public string SellerId { get; }
    public string Title { get; }
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public ListingCreatedDomainEvent(string listingId, string sellerId, string title)
    {
        ListingId = listingId;
        SellerId = sellerId;
        Title = title;
    }
}