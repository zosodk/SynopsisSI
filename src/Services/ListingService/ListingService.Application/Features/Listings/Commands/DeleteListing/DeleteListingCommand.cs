using System.ComponentModel.DataAnnotations;
namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.DeleteListing;
public class DeleteListingCommand
{
    [Required] public string Id { get; set; } = string.Empty;
    [Required] public string SellerId { get; set; } = string.Empty; 
}