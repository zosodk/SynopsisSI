        using Microsoft.AspNetCore.Mvc;
        using SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.CreateListing;
        using SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.GetListingById;
        using SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.UpdateListing;
        using SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.DeleteListing;
        using SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.SearchListings;
        using System;
        using System.Net.Mime;
        using System.Threading;
        using System.Threading.Tasks;
        using Microsoft.AspNetCore.Http;
        using Microsoft.Extensions.Logging;
        using Microsoft.EntityFrameworkCore; 
        using System.ComponentModel.DataAnnotations; 

        namespace SynopsisSI.Services.ListingService.API.Controllers;

        [ApiController]
        [Route("api/[controller]")]
        [Produces(MediaTypeNames.Application.Json)]
        public class ListingsController : ControllerBase
        {
            private readonly CreateListingCommandHandler _createListingHandler;
            private readonly GetListingByIdQueryHandler _getListingByIdHandler;
            private readonly UpdateListingCommandHandler _updateListingHandler;
            private readonly DeleteListingCommandHandler _deleteListingHandler;
            private readonly SearchListingsQueryHandler _searchListingsHandler;
            private readonly ILogger<ListingsController> _logger;

            public ListingsController(
                CreateListingCommandHandler createListingHandler,
                GetListingByIdQueryHandler getListingByIdHandler,
                UpdateListingCommandHandler updateListingHandler,
                DeleteListingCommandHandler deleteListingHandler,
                SearchListingsQueryHandler searchListingsHandler,
                ILogger<ListingsController> logger)
            {
                _createListingHandler = createListingHandler ?? throw new ArgumentNullException(nameof(createListingHandler));
                _getListingByIdHandler = getListingByIdHandler ?? throw new ArgumentNullException(nameof(getListingByIdHandler));
                _updateListingHandler = updateListingHandler ?? throw new ArgumentNullException(nameof(updateListingHandler));
                _deleteListingHandler = deleteListingHandler ?? throw new ArgumentNullException(nameof(deleteListingHandler));
                _searchListingsHandler = searchListingsHandler ?? throw new ArgumentNullException(nameof(searchListingsHandler));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            [HttpPost]
            [Consumes(MediaTypeNames.Application.Json)]
            [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<IActionResult> CreateListing([FromBody] CreateListingCommand command, CancellationToken cancellationToken)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                try
                {
                    var listingId = await _createListingHandler.Handle(command, cancellationToken);
                    return CreatedAtAction(nameof(GetListingById), new { id = listingId }, new { id = listingId });
                }
                catch (ArgumentException ex) { _logger.LogWarning(ex, "ArgEx creating listing."); return BadRequest(new { error = ex.Message }); }
                catch (ApplicationException ex) { _logger.LogWarning(ex, "AppEx creating listing."); return BadRequest(new { error = ex.Message }); }
                catch (Exception ex) { _logger.LogError(ex, "Error creating listing."); return StatusCode(StatusCodes.Status500InternalServerError, "Error creating listing."); }
            }

            [HttpGet("{id}")]
            [ProducesResponseType(typeof(ListingItemDto), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<ActionResult<ListingItemDto>> GetListingById(string id, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "Listing ID required."});
                try
                {
                    var dto = await _getListingByIdHandler.Handle(new GetListingByIdQuery { Id = id }, cancellationToken);
                    return dto == null ? NotFound(new { message = $"Listing {id} not found."}) : Ok(dto);
                }
                catch (ArgumentException ex) { _logger.LogWarning(ex, "ArgEx getting listing {Id}.", id); return BadRequest(new { error = ex.Message }); }
                catch (Exception ex) { _logger.LogError(ex, "Error getting listing {Id}.", id); return StatusCode(StatusCodes.Status500InternalServerError, "Error getting listing.");}
            }

            [HttpPut("{id}")]
            [Consumes(MediaTypeNames.Application.Json)]
            [ProducesResponseType(StatusCodes.Status204NoContent)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(StatusCodes.Status403Forbidden)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            [ProducesResponseType(StatusCodes.Status409Conflict)]
            public async Task<IActionResult> UpdateListing(string id, [FromBody] UpdateListingCommand command, CancellationToken cancellationToken)
            {
                if (id != command.Id) return BadRequest(new { error = "Route ID and command ID mismatch." });
                if (!ModelState.IsValid) return BadRequest(ModelState);
                try
                {
                    var success = await _updateListingHandler.Handle(command, cancellationToken);
                    return success ? NoContent() : NotFound(new { message = $"Listing {id} not found for update." });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Concurrency conflict updating listing {ListingId}.", id);
                    return Conflict(new { error = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogWarning(ex, "Unauthorized update attempt for listing {ListingId}.", id);
                    return Forbid(); 
                }
                catch (ArgumentException ex) { _logger.LogWarning(ex, "ArgEx updating listing {Id}.", id); return BadRequest(new { error = ex.Message }); }
                catch (Exception ex) { _logger.LogError(ex, "Error updating listing {Id}.", id); return StatusCode(StatusCodes.Status500InternalServerError, "Error updating listing."); }
            }

            [HttpDelete("{id}")]
            [ProducesResponseType(StatusCodes.Status204NoContent)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            [ProducesResponseType(StatusCodes.Status401Unauthorized)]
            [ProducesResponseType(StatusCodes.Status403Forbidden)]
            [ProducesResponseType(StatusCodes.Status404NotFound)]
            public async Task<IActionResult> DeleteListing(string id, [FromQuery, Required] string sellerId, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(sellerId))
                {
                     return BadRequest(new { error = "Listing ID and Seller ID must be provided." });
                }
                var command = new DeleteListingCommand { Id = id, SellerId = sellerId };
                try
                {
                    var success = await _deleteListingHandler.Handle(command, cancellationToken);
                    return success ? NoContent() : NotFound(new { message = $"Listing {id} not found for deletion." });
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogWarning(ex, "Unauthorized delete attempt for listing {ListingId}.", id);
                    return Forbid();
                }
                catch (Exception ex) { _logger.LogError(ex, "Error deleting listing {Id}.", id); return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting listing."); }
            }

            [HttpGet]
            [ProducesResponseType(typeof(PagedListingsResultDto), StatusCodes.Status200OK)]
            [ProducesResponseType(StatusCodes.Status400BadRequest)]
            public async Task<ActionResult<PagedListingsResultDto>> SearchListings([FromQuery] SearchListingsQuery query, CancellationToken cancellationToken)
            {
                if (query.PageNumber < 1) query.PageNumber = 1;
                if (query.PageSize < 1) query.PageSize = 10;
                if (query.PageSize > 100) query.PageSize = 100;

                try
                {
                    var result = await _searchListingsHandler.Handle(query, cancellationToken);
                    return Ok(result);
                }
                catch (Exception ex) { _logger.LogError(ex, "Error searching listings: {@Query}", query); return StatusCode(StatusCodes.Status500InternalServerError, "Error searching listings."); }
            }
        }
