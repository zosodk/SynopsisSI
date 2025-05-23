    using System;
    using System.Collections.Generic;
    using SynopsisSI.Services.ListingService.Domain.ValueObjects; //Location

    namespace SynopsisSI.Services.ListingService.Domain.Entities;

    public class ListingItem // Could inherit from a local BaseEntity if you create one here
    {
        public string Id { get; private set; } // Typically a GUID string or MongoDB ObjectId string
        public string SellerId { get; private set; } // ID of the user who owns this listing
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Category { get; private set; } // Could be an Enum or a Value Object
        public decimal Price { get; private set; }
        public string Currency { get; private set; } // E.g., "USD", "EUR"
        public string Condition { get; private set; } // E.g., "New", "UsedLikeNew", "UsedGood"
        public Dictionary<string, object> ItemSpecifics { get; private set; }
        public List<string> ImageUrls { get; private set; } // URLs pointing to images in cloud storage
        public ListingStatus Status { get; private set; }
        public List<string>? Tags { get; private set; }
        public GeoLocation? Location { get; private set; } // Value Object for location
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public int Version { get; private set; } // For optimistic concurrency

        // Private constructor for EF Core and factory methods
        private ListingItem()
        {
            Id = Guid.NewGuid().ToString(); // Default ID generation
            ItemSpecifics = new Dictionary<string, object>();
            ImageUrls = new List<string>();
            Tags = new List<string>();
            Status = ListingStatus.Draft; // Default to Draft
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
            // Basic validation
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
                Status = ListingStatus.Available, // Set to Available on creation, or Draft then Published
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            // Add domain events here if applicable (e.g., ListingCreatedDomainEvent)
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
            Version++; // Increment version for optimistic concurrency
        }

        public void UpdateImageUrls(List<string> newImageUrls)
        {
            ImageUrls = newImageUrls ?? new List<string>();
            UpdatedAt = DateTime.UtcNow;
            Version++;
        }

        public void MarkAsSold(string? orderId = null) // orderId is for the ListingStatusChangedEvent
        {
            if (Status == ListingStatus.Sold) throw new InvalidOperationException("Listing is already sold.");
            Status = ListingStatus.Sold;
            UpdatedAt = DateTime.UtcNow;
            Version++;
            // Add domain event: ListingSoldDomainEvent (could include orderId)
        }
         public void MarkAsReserved()
        {
            if (Status != ListingStatus.Available) throw new InvalidOperationException($"Cannot reserve listing with status {Status}.");
            Status = ListingStatus.Reserved;
            UpdatedAt = DateTime.UtcNow;
            Version++;
        }

        public void MarkAsAvailable() // e.g., if a reservation expires or an order is cancelled
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