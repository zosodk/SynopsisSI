        using SynopsisSI.Services.ListingService.Application.Interfaces.Persistence;
        using Microsoft.Extensions.Logging;
        using System;
        using System.Threading;
        using System.Threading.Tasks;

        namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.DeleteListing;

        public class DeleteListingCommandHandler
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly ILogger<DeleteListingCommandHandler> _logger;

            public DeleteListingCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteListingCommandHandler> logger)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<bool> Handle(DeleteListingCommand request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Handling DeleteListingCommand for ListingId: {ListingId}, SellerId: {SellerId}", request.Id, request.SellerId);
                var listingItem = await _unitOfWork.Listings.GetByIdAsync(request.Id, cancellationToken);
                if (listingItem == null)
                {
                    _logger.LogWarning("Listing not found for deletion. ListingId: {ListingId}", request.Id);
                    return false; // Or throw NotFoundException
                }

                if (listingItem.SellerId != request.SellerId)
                {
                     _logger.LogWarning("Unauthorized attempt to delete ListingId: {ListingId} by SellerId: {RequesterSellerId}", request.Id, request.SellerId);
                    throw new UnauthorizedAccessException("User is not authorized to delete this listing.");
                }

                listingItem.Delist(); // Soft delete by changing status
                await _unitOfWork.Listings.UpdateAsync(listingItem, cancellationToken);
                // If hard delete: await _unitOfWork.Listings.DeleteAsync(listingItem, cancellationToken);
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Listing marked as delisted successfully. ListingId: {ListingId}", request.Id);
                // TODO: Publish ListingDelistedEvent
                return true;
            }
        }