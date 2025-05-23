namespace SynopsisSI.Services.ListingService.Domain.Entities;

public enum ListingStatus
{
    Draft,      // Item created but not yet visible for sale
    Available,  // Item is listed and available for purchase
    Reserved,   // Item is temporarily reserved (e.g., pending payment)
    Sold,       // Item has been sold
    Delisted    // Item has been removed from sale by the seller or admin
}