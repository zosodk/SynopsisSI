using System.Collections.Generic;
namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.SearchListings;
public class SearchListingsQuery
{
    public string? Keyword { get; set; }
    public string? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Condition { get; set; }
    public double? CenterLatitude { get; set; }
    public double? CenterLongitude { get; set; }
    public double? RadiusKm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } // e.g., "price_asc", "date_desc"
}