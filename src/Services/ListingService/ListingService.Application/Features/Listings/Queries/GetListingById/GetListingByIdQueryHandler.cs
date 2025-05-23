using SynopsisSI.Services.ListingService.Application.Interfaces.Persistence;
        // using SynopsisSI.Services.ListingService.Application.Interfaces.Infrastructure; // For ICacheService if used here
        using SynopsisSI.Services.ListingService.Domain.Entities; // For ListingStatus enum
        using Microsoft.Extensions.Logging;
        using System;
        using System.Threading;
        using System.Threading.Tasks;
        using System.Collections.Generic; // For Dictionary
        // using MediatR;
        // using AutoMapper; // If using AutoMapper

        namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.GetListingById;

        public class GetListingByIdQueryHandler // : IRequestHandler<GetListingByIdQuery, ListingItemDto?>
        {
            private readonly IListingRepository _listingRepository;
            private readonly ILogger<GetListingByIdQueryHandler> _logger;
            // private readonly IMapper _mapper; // If using AutoMapper
            // private readonly ICacheService _cacheService;

            public GetListingByIdQueryHandler(
                IListingRepository listingRepository,
                ILogger<GetListingByIdQueryHandler> logger
                /*, IMapper mapper, ICacheService cacheService */)
            {
                _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                // _mapper = mapper;
                // _cacheService = cacheService;
            }

            public async Task<ListingItemDto?> Handle(GetListingByIdQuery request, CancellationToken cancellationToken)
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Id))
                {
                    _logger.LogWarning("GetListingByIdQuery handled with invalid request (null or empty ID).");
                    throw new ArgumentException("Listing ID must be provided.", nameof(request.Id));
                }

                // string cacheKey = $"listing:{request.Id}";
                // var cachedDto = await _cacheService.GetAsync<ListingItemDto>(cacheKey);
                // if (cachedDto != null) {
                //     _logger.LogInformation("Cache hit for ListingId: {ListingId}", request.Id);
                //     return cachedDto;
                // }
                // _logger.LogInformation("Cache miss for ListingId: {ListingId}", request.Id);


                var listingItem = await _listingRepository.GetByIdAsync(request.Id, cancellationToken);
                if (listingItem == null)
                {
                    _logger.LogInformation("Listing with ID {ListingId} not found.", request.Id);
                    return null;
                }

                // Manual mapping or AutoMapper
                var listingDto = new ListingItemDto
                {
                    Id = listingItem.Id,
                    SellerId = listingItem.SellerId,
                    Title = listingItem.Title,
                    Description = listingItem.Description,
                    Category = listingItem.Category,
                    Price = listingItem.Price,
                    Currency = listingItem.Currency,
                    Condition = listingItem.Condition,
                    ItemSpecifics = listingItem.ItemSpecifics,
                    ImageUrls = listingItem.ImageUrls,
                    Status = listingItem.Status.ToString(), // Convert enum to string
                    Tags = listingItem.Tags,
                    Location = listingItem.Location != null
                        ? new GeoLocationDto { Longitude = listingItem.Location.Longitude, Latitude = listingItem.Location.Latitude }
                        : null,
                    CreatedAt = listingItem.CreatedAt,
                    UpdatedAt = listingItem.UpdatedAt
                };
                // var listingDto = _mapper.Map<ListingItemDto>(listingItem); // If using AutoMapper

                // await _cacheService.SetAsync(cacheKey, listingDto, TimeSpan.FromMinutes(10));
                _logger.LogInformation("Listing with ID {ListingId} retrieved.", request.Id);
                return listingDto;
            }
        }