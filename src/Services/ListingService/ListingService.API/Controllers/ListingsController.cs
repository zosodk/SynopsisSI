    using Microsoft.AspNetCore.Mvc;
    using SynopsisSI.Services.ListingService.Application.Features.Listings.Commands.CreateListing;
    using SynopsisSI.Services.ListingService.Application.Features.Listings.Queries.GetListingById;
    using System;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    namespace SynopsisSI.Services.ListingService.API.Controllers;

    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ListingsController : ControllerBase
    {
        private readonly CreateListingCommandHandler _createListingHandler;
        private readonly GetListingByIdQueryHandler _getListingByIdHandler;
        private readonly ILogger<ListingsController> _logger;

        public ListingsController(
            CreateListingCommandHandler createListingHandler,
            GetListingByIdQueryHandler getListingByIdHandler,
            ILogger<ListingsController> logger)
        {
            _createListingHandler = createListingHandler ?? throw new ArgumentNullException(nameof(createListingHandler));
            _getListingByIdHandler = getListingByIdHandler ?? throw new ArgumentNullException(nameof(getListingByIdHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateListing([FromBody] CreateListingCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("CreateListing called with invalid model state.");
                return BadRequest(ModelState);
            }
            try
            {
                var listingId = await _createListingHandler.Handle(command, cancellationToken);
                _logger.LogInformation("Listing created successfully with ID: {ListingId}", listingId);
                return CreatedAtAction(nameof(GetListingById), new { id = listingId }, new { id = listingId });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error during listing creation: {ErrorMessage}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (ApplicationException ex)
            {
                 _logger.LogWarning(ex, "Application error during listing creation: {ErrorMessage}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating listing: {@Command}", command);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred creating the listing.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ListingItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ListingItemDto>> GetListingById(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogWarning("GetListingById called with empty ID.");
                return BadRequest(new { error = "Listing ID cannot be empty."});
            }
            try
            {
                var query = new GetListingByIdQuery { Id = id };
                var listingDto = await _getListingByIdHandler.Handle(query, cancellationToken);

                if (listingDto == null)
                {
                    _logger.LogInformation("Listing with ID {ListingId} not found.", id);
                    return NotFound(new { message = $"Listing with ID {id} not found." });
                }
                _logger.LogInformation("Listing with ID {ListingId} retrieved.", id);
                return Ok(listingDto);
            }
            catch (ArgumentException ex)
            {
                 _logger.LogWarning(ex, "Argument error retrieving listing {ListingId}: {ErrorMessage}", id, ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving listing {ListingId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred retrieving the listing.");
            }
        }
    }