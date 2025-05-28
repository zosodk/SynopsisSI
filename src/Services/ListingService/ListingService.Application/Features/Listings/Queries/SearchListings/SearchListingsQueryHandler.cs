        using SynopsisSI.Services.ListingService.Application.Interfaces.Persistence;
        using SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.GetListingById;
        using SynopsisSI.Services.ListingService.Domain.Entities;
        using Microsoft.Extensions.Logging;
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Linq.Expressions;
        using System.Threading;
        using System.Threading.Tasks;

        namespace SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.SearchListings;

        public class SearchListingsQueryHandler
        {
            private readonly IListingRepository _listingRepository;
            private readonly ILogger<SearchListingsQueryHandler> _logger;

            public SearchListingsQueryHandler(IListingRepository listingRepository, ILogger<SearchListingsQueryHandler> logger)
            {
                _listingRepository = listingRepository ?? throw new ArgumentNullException(nameof(listingRepository));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<PagedListingsResultDto> Handle(SearchListingsQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Handling SearchListingsQuery: {@SearchQuery}", request);

                Expression<Func<ListingItem, bool>> predicate = l => l.Status == ListingStatus.Available;

                if (!string.IsNullOrWhiteSpace(request.Keyword))
                {
                    var keywordLower = request.Keyword.ToLowerInvariant();
                    predicate = predicate.And(l => 
                        (l.Title != null && l.Title.ToLower().Contains(keywordLower)) || 
                        (l.Description != null && l.Description.ToLower().Contains(keywordLower)) ||
                        (l.Tags != null && l.Tags.Any(t => t.ToLower().Contains(keywordLower)))
                    );
                }
                if (!string.IsNullOrWhiteSpace(request.Category))
                {
                    predicate = predicate.And(l => l.Category.Equals(request.Category, StringComparison.OrdinalIgnoreCase));
                }
                if (request.MinPrice.HasValue)
                {
                    predicate = predicate.And(l => l.Price >= request.MinPrice.Value);
                }
                if (request.MaxPrice.HasValue)
                {
                    predicate = predicate.And(l => l.Price <= request.MaxPrice.Value);
                }
                if (!string.IsNullOrWhiteSpace(request.Condition))
                {
                    predicate = predicate.And(l => l.Condition.Equals(request.Condition, StringComparison.OrdinalIgnoreCase));
                }
                
                int skip = (request.PageNumber - 1) * request.PageSize;
                int take = request.PageSize;

                // For production, implement proper counting in repository or use a search engine.
                var allMatchingListingsForCount = await _listingRepository.FindAsync(predicate, 0, int.MaxValue, cancellationToken); 
                var totalCount = allMatchingListingsForCount.Count;
                
                // Add sorting logic here based on request.SortBy before pagination
                // Eksempelvis: if (request.SortBy == "price_asc") allMatchingListingsForCount = allMatchingListingsForCount.OrderBy(l => l.Price).ToList();
                
                var listings = allMatchingListingsForCount.Skip(skip).Take(take).ToList();


                var listingDtos = listings.Select(l => new ListingItemDto 
                {
                    Id = l.Id, SellerId = l.SellerId, Title = l.Title, Description = l.Description,
                    Category = l.Category, Price = l.Price, Currency = l.Currency, Condition = l.Condition,
                    ItemSpecifics = l.ItemSpecifics, ImageUrls = l.ImageUrls, Status = l.Status.ToString(),
                    Tags = l.Tags,
                    Location = l.Location != null ? new GeoLocationDto { Longitude = l.Location.Longitude, Latitude = l.Location.Latitude } : null,
                    CreatedAt = l.CreatedAt, UpdatedAt = l.UpdatedAt, Version = l.Version
                }).ToList();

                var result = new PagedListingsResultDto
                {
                    Items = listingDtos,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalCount = totalCount
                };

                _logger.LogInformation("SearchListingsQuery returned {ItemCount} items for page {PageNumber} of {TotalPages} total pages.", result.Items.Count, result.PageNumber, result.TotalPages);
                return result;
            }
        }

        public static class PredicateBuilder
        {
            public static Expression<Func<T, bool>> True<T>() { return f => true; }
            public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
            {
                var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
                return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
            }
        }