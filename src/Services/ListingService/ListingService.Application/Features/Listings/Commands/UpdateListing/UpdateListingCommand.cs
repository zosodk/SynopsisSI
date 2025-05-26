        using System.Collections.Generic;
        using System.ComponentModel.DataAnnotations;

        namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.UpdateListing;

        public class UpdateListingCommand
        {
            [Required]
            public string Id { get; set; } = string.Empty;

            [Required(ErrorMessage = "Seller ID is required for authorization.")]
            public string SellerId { get; set; } = string.Empty;

            [Required(ErrorMessage = "Title is required.")]
            [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "Description is required.")]
            [StringLength(10000, ErrorMessage = "Description cannot exceed 10000 characters.")]
            public string Description { get; set; } = string.Empty;

            [Required(ErrorMessage = "Category is required.")]
            public string Category { get; set; } = string.Empty;

            [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "Currency is required.")]
            [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be 3 characters.")]
            public string Currency { get; set; } = string.Empty;

            [Required(ErrorMessage = "Condition is required.")]
            public string Condition { get; set; } = string.Empty;

            public Dictionary<string, object>? ItemSpecifics { get; set; }
            public List<string>? ImageObjectKeys { get; set; }
            public double? LocationLongitude { get; set; }
            public double? LocationLatitude { get; set; }
            public List<string>? Tags { get; set; }

            [Required(ErrorMessage = "Version is required for optimistic concurrency.")]
            public int Version { get; set; }
        }