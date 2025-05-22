using SynopsisSI.Services.ListingService.Application.Interfaces.Persistence;
        using SynopsisSI.Services.ListingService.Application.Interfaces.Infrastructure; // For ICloudStorageService
        using SynopsisSI.Services.ListingService.Domain.Entities;
        using SynopsisSI.Services.ListingService.Domain.ValueObjects; // For GeoLocation
        using Microsoft.Extensions.Logging;
        using System;
        using System.Linq;
        using System.Threading;
        using System.Threading.Tasks;
        using System.Collections.Generic; // For Dictionary, List
        // using MediatR; // If using MediatR: public class CreateListingCommandHandler : IRequestHandler<CreateListingCommand, string>
        // using SynopsisSI.Shared.Events; // If publishing ListingStatusChangedEvent

        namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.CreateListing;

        public class CreateListingCommandHandler // : IRequestHandler<CreateListingCommand, string>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly ICloudStorageService _cloudStorageService; // To construct full URLs
            private readonly ILogger<CreateListingCommandHandler> _logger;
            // private readonly IPublisher _mediator; // If publishing domain or integration events

            public CreateListingCommandHandler(
                IUnitOfWork unitOfWork,
                ICloudStorageService cloudStorageService,
                ILogger<CreateListingCommandHandler> logger
                /*, IPublisher mediator (if using MediatR) */)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _cloudStorageService = cloudStorageService ?? throw new ArgumentNullException(nameof(cloudStorageService));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                // _mediator = mediator;
            }

            public async Task<string> Handle(CreateListingCommand request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Handling CreateListingCommand for SellerId: {SellerId}, Title: {Title}", request.SellerId, request.Title);

                // In a real User microservice scenario, SellerId existence would be checked,
                // possibly via an API call or by an event a user service publishes.
                // For now, we assume SellerId is valid.

                GeoLocation? location = null;
                if (request.LocationLongitude.HasValue && request.LocationLatitude.HasValue)
                {
                    location = GeoLocation.FromCoordinates(request.LocationLongitude.Value, request.LocationLatitude.Value);
                }

                var imageUrls = request.ImageObjectKeys?
                    .Select(key => _cloudStorageService.GetPublicUrl(key)) // Construct full URLs
                    .ToList() ?? new List<string>();

                var listingItem = ListingItem.Create(
                    sellerId: request.SellerId,
                    title: request.Title,
                    description: request.Description,
                    category: request.Category,
                    price: request.Price,
                    currency: request.Currency,
                    condition: request.Condition,
                    itemSpecifics: request.ItemSpecifics,
                    imageUrls: imageUrls, // Use constructed URLs
                    tags: request.Tags,
                    location: location
                );
                // ListingItem.Create sets initial status, e.g., to Available.

                await _unitOfWork.Listings.AddAsync(listingItem, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Listing created successfully. ID: {ListingId}", listingItem.Id);

                // publish an integration event (e.g., ListingCreatedIntegrationEvent)
                // would be defined in SynopsisSI.Shared.Events and sent via a message broker.
                // var listingCreatedEvent = new ListingStatusChangedEvent // a specific ListingCreatedEvent
                // {
                //     ListingId = listingItem.Id,
                //     NewStatus = listingItem.Status.ToString(),
                //     OldStatus = "None" // Or initial state
                // };
                // await _messageBus.PublishAsync(listingCreatedEvent, cancellationToken);

                return listingItem.Id;
            }
        }