using SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.GetListingById; // For ListingItemDto
using System.Collections.Generic;
namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.SearchListings;
public class PagedListingsResultDto
{
    public List<ListingItemDto> Items { get; set; } = new List<ListingItemDto>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => TotalCount > 0 && PageSize > 0 ? (int)System.Math.Ceiling(TotalCount / (double)PageSize) : 0;
}