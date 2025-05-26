using System;
namespace SynopsisSI.Services.OrderService.Domain.Entities;
public class OrderItem
{
    public string ListingId { get; private set; }
    public string ProductTitleSnapshot { get; private set; }
    public decimal PriceAtPurchase { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { ListingId = string.Empty; ProductTitleSnapshot = string.Empty;}
    public static OrderItem Create(string listingId, string productTitleSnapshot, decimal priceAtPurchase, int quantity)
    {
        if (string.IsNullOrWhiteSpace(listingId)) throw new ArgumentNullException(nameof(listingId));
        if (string.IsNullOrWhiteSpace(productTitleSnapshot)) throw new ArgumentNullException(nameof(productTitleSnapshot));
        if (priceAtPurchase < 0) throw new ArgumentOutOfRangeException(nameof(priceAtPurchase));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        return new OrderItem {
            ListingId = listingId, ProductTitleSnapshot = productTitleSnapshot,
            PriceAtPurchase = priceAtPurchase, Quantity = quantity
        };
    }
}