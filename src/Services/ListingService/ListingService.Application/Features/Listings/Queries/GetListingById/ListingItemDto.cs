using System;
using System.Collections.Generic;

namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.GetListingById;

public class ListingItemDto
{
    public string Id { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public Dictionary<string, object> ItemSpecifics { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public List<string>? Tags { get; set; }
    public GeoLocationDto? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Version { get; set; } // <-- Ensure this property exists
}

public class GeoLocationDto
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }
}