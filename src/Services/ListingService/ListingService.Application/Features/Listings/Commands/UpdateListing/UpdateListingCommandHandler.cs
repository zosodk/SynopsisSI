        using SynopsisSI.Services.ListingService.Application.Interfaces.Persistence;
        using SynopsisSI.Services.ListingService.Application.Interfaces.Infrastructure;
        using SynopsisSI.Services.ListingService.Domain.ValueObjects;
        using Microsoft.Extensions.Logging;
        using System;
        using System.Linq;
        using System.Threading;
        using System.Threading.Tasks;
        using Microsoft.EntityFrameworkCore; // For DbUpdateConcurrencyException
        using System.Collections.Generic;

        namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.UpdateListing;

        public class UpdateListingCommandHandler
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly ICloudStorageService _cloudStorageService;
            private readonly ILogger<UpdateListingCommandHandler> _logger;

            public UpdateListingCommandHandler(IUnitOfWork unitOfWork, ICloudStorageService cloudStorageService, ILogger<UpdateListingCommandHandler> logger)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _cloudStorageService = cloudStorageService ?? throw new ArgumentNullException(nameof(cloudStorageService));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<bool> Handle(UpdateListingCommand request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Handling UpdateListingCommand for ListingId: {ListingId}, SellerId: {SellerId}", request.Id, request.SellerId);

                var listingItem = await _unitOfWork.Listings.GetByIdAsync(request.Id, cancellationToken);
                if (listingItem == null)
                {
                    _logger.LogWarning("Listing not found for update. ListingId: {ListingId}", request.Id);
                    return false;
                }

                if (listingItem.SellerId != request.SellerId)
                {
                    _logger.LogWarning("Unauthorized attempt to update ListingId: {ListingId} by SellerId: {RequesterSellerId}. Actual SellerId: {ActualSellerId}",
                        request.Id, request.SellerId, listingItem.SellerId);
                    throw new UnauthorizedAccessException("User is not authorized to update this listing.");
                }
                
                if (listingItem.Version != request.Version)
                {
                    _logger.LogWarning("Concurrency conflict updating ListingId: {ListingId}. Request Version: {RequestVersion}, DB Version: {DbVersion}",
                        request.Id, request.Version, listingItem.Version);
                    throw new DbUpdateConcurrencyException($"The listing has been modified by another user. Please refresh and try again. Expected version {request.Version}, found {listingItem.Version}.");
                }

                GeoLocation? location = null;
                if (request.LocationLongitude.HasValue && request.LocationLatitude.HasValue)
                {
                    location = GeoLocation.FromCoordinates(request.LocationLongitude.Value, request.LocationLatitude.Value);
                }

                var imageUrls = request.ImageObjectKeys?
                    .Where(key => !string.IsNullOrWhiteSpace(key))
                    .Select(key => _cloudStorageService.GetPublicUrl(key.Trim()))
                    .ToList();

                listingItem.UpdateDetails(
                    request.Title, request.Description, request.Category, request.Price, request.Currency,
                    request.Condition, request.ItemSpecifics, request.Tags, location
                );
                
                if (request.ImageObjectKeys != null)
                {
                    listingItem.UpdateImageUrls(imageUrls ?? new List<string>());
                }

                await _unitOfWork.Listings.UpdateAsync(listingItem, cancellationToken); 
                
                try
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Listing updated successfully. ListingId: {ListingId}", request.Id);
                    return true;
                }
                catch (DbUpdateConcurrencyException ex) 
                {
                    _logger.LogWarning(ex, "Concurrency conflict during SaveChanges for ListingId: {ListingId}. Version mismatch.", request.Id);
                    throw; 
                }
            }
        }