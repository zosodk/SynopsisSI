using System;
using System.Collections.Generic;
using SynopsisSI.Services.ListingService.Domain.ValueObjects;

namespace SynopsisSI.Services.ListingService.Domain.Entities;

public class ListingItem
{
    public string Id { get; private set; }
    public string SellerId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; }
    public string Condition { get; private set; }
    public Dictionary<string, object> ItemSpecifics { get; private set; }
    public List<string> ImageUrls { get; private set; }
    public ListingStatus Status { get; private set; }
    public List<string>? Tags { get; private set; }
    public GeoLocation? Location { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public int Version { get; private set; }

    private ListingItem()
    {
        Id = Guid.NewGuid().ToString();
        ItemSpecifics = new Dictionary<string, object>();
        ImageUrls = new List<string>();
        Tags = new List<string>();
        Status = ListingStatus.Draft;
        Version = 1;
    }

    public static ListingItem Create(
        string sellerId,
        string title,
        string description,
        string category,
        decimal price,
        string currency,
        string condition,
        Dictionary<string, object>? itemSpecifics,
        List<string>? imageUrls,
        List<string>? tags,
        GeoLocation? location)
    {
        if (string.IsNullOrWhiteSpace(sellerId)) throw new ArgumentException("Seller ID cannot be empty.", nameof(sellerId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        var listing = new ListingItem
        {
            SellerId = sellerId,
            Title = title,
            Description = description ?? string.Empty,
            Category = category ?? "Uncategorized",
            Price = price,
            Currency = currency ?? "USD",
            Condition = condition ?? "Used",
            ItemSpecifics = itemSpecifics ?? new Dictionary<string, object>(),
            ImageUrls = imageUrls ?? new List<string>(),
            Tags = tags?.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList() ?? new List<string>(),
            Location = location,
            Status = ListingStatus.Available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return listing;
    }

    public void UpdateDetails(
        string title, string description, string category, decimal price, string currency,
        string condition, Dictionary<string, object>? itemSpecifics, List<string>? tags, GeoLocation? location)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title cannot be empty.", nameof(title));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");

        Title = title;
        Description = description ?? string.Empty;
        Category = category ?? "Uncategorized";
        Price = price;
        Currency = currency ?? "USD";
        Condition = condition ?? "Used";
        ItemSpecifics = itemSpecifics ?? new Dictionary<string, object>();
        Tags = tags?.Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList() ?? new List<string>();
        Location = location;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    public void UpdateImageUrls(List<string> newImageUrls)
    {
        ImageUrls = newImageUrls ?? new List<string>();
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    public void MarkAsSold(string? orderId = null)
    {
        if (Status == ListingStatus.Sold) throw new InvalidOperationException("Listing is already sold.");
        Status = ListingStatus.Sold;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    public void MarkAsReserved()
    {
        if (Status != ListingStatus.Available) throw new InvalidOperationException($"Cannot reserve listing with status {Status}.");
        Status = ListingStatus.Reserved;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    public void MarkAsAvailable()
    {
        if (Status == ListingStatus.Sold) throw new InvalidOperationException("Cannot mark a sold listing as available through this method.");
        Status = ListingStatus.Available;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }

    public void Delist()
    {
        Status = ListingStatus.Delisted;
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }
}