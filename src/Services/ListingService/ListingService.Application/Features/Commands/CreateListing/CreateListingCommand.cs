using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// using MediatR;
// If using MediatR: public class CreateListingCommand : IRequest<string>

namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.CreateListing;

public class CreateListingCommand // : IRequest<string>
{
    [Required] public string SellerId { get; set; } = string.Empty;
    [Required, StringLength(150, MinimumLength = 3)] public string Title { get; set; } = string.Empty;
    [Required, StringLength(10000)] public string Description { get; set; } = string.Empty;
    [Required] public string Category { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue)] public decimal Price { get; set; }
    [Required, StringLength(3, MinimumLength = 3)] public string Currency { get; set; } = "USD";
    [Required] public string Condition { get; set; } = string.Empty;
    public Dictionary<string, object>? ItemSpecifics { get; set; }
    public List<string>? ImageObjectKeys { get; set; } // Keys from S3/MinIO after client uploads
    public double? LocationLongitude { get; set; }
    public double? LocationLatitude { get; set; }
    public List<string>? Tags { get; set; }
}