        using MassTransit;
        using Microsoft.Extensions.Logging;
        using SynopsisSI.Services.ListingService.Application.Interfaces.Persistence;
        using SynopsisSI.Shared.Events;
        using System.Threading.Tasks;
        using System;
        using System.Linq;
        using SynopsisSI.Services.ListingService.Domain.Entities;
        using Microsoft.EntityFrameworkCore;

        namespace SynopsisSI.Services.ListingService.Application.Features.Listings.EventConsumers;

        public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly ILogger<OrderPlacedEventConsumer> _logger;

            public OrderPlacedEventConsumer(IUnitOfWork unitOfWork, ILogger<OrderPlacedEventConsumer> logger)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
            {
                var orderEvent = context.Message;
                _logger.LogInformation("ListingService received OrderPlacedEvent for OrderId: {OrderId}. Processing {ItemCount} listing items.",
                    orderEvent.OrderId, orderEvent.Items.Count);

                if (orderEvent.Items == null || !orderEvent.Items.Any())
                {
                    _logger.LogWarning("OrderPlacedEvent for OrderId {OrderId} has no items. No listings to update.", orderEvent.OrderId);
                    return;
                }

                bool changesMade = false;
                foreach (var itemDetail in orderEvent.Items)
                {
                    var listing = await _unitOfWork.Listings.GetByIdAsync(itemDetail.ListingId, context.CancellationToken);
                    if (listing != null)
                    {
                        if (listing.Status == ListingStatus.Available || listing.Status == ListingStatus.Reserved)
                        {
                            try
                            {
                                listing.MarkAsSold(orderEvent.OrderId);
                                await _unitOfWork.Listings.UpdateAsync(listing, context.CancellationToken);
                                changesMade = true;
                                _logger.LogInformation("ListingId {ListingId} marked as Sold due to OrderId {OrderId}.", itemDetail.ListingId, orderEvent.OrderId);
                            }
                            catch (InvalidOperationException ex) 
                            {
                                _logger.LogWarning(ex, "Domain rule violation: Could not mark ListingId {ListingId} as Sold for OrderId {OrderId}. Current status: {Status}", itemDetail.ListingId, orderEvent.OrderId, listing.Status);
                            }
                            catch (DbUpdateConcurrencyException ex)
                            {
                                _logger.LogWarning(ex, "Concurrency conflict when trying to mark ListingId {ListingId} as Sold for OrderId {OrderId}.", itemDetail.ListingId, orderEvent.OrderId);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("ListingId {ListingId} (Order {OrderId}) was not in an 'Available' or 'Reserved' state. Current status: {Status}. No action taken.", 
                                itemDetail.ListingId, orderEvent.OrderId, listing.Status);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("ListingId {ListingId} from OrderPlacedEvent (OrderId: {OrderId}) not found in ListingService.",
                            itemDetail.ListingId, orderEvent.OrderId);
                    }
                }

                if (changesMade)
                {
                    try
                    {
                        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
                        _logger.LogInformation("Successfully processed and saved listing status updates for OrderPlacedEvent: {OrderId}", orderEvent.OrderId);
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                         _logger.LogWarning(ex, "Concurrency conflict during final SaveChanges for OrderPlacedEvent {OrderId}.", orderEvent.OrderId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving changes after processing OrderPlacedEvent {OrderId}", orderEvent.OrderId);
                        throw; 
                    }
                }
                else
                {
                     _logger.LogInformation("No listing status changes made for OrderPlacedEvent: {OrderId}", orderEvent.OrderId);
                }
            }
        }