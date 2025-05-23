// using MediatR;

namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.GetListingById;

public class GetListingByIdQuery // : IRequest<ListingItemDto?>
{
    public string Id { get; set; } = string.Empty;
}