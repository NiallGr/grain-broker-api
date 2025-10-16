using GrainBroker.Core.DTOs;
using GrainBroker.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GrainBroker.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orders;

        public OrdersController(IOrderService orders) => _orders = orders;
        /// <summary>
        /// Returns a paged list of orders with optional free-text and date-range filtering.
        /// </summary>
        /// <param name="page">1-based page index (default 1)</param>
        /// <param name="pageSize">Page size 1..200 (default 50)</param>
        /// <param name="q">Free-text query</param>
        /// <param name="from">Start date (inclusive)</param>
        /// <param name="to">End date (inclusive)</param>
        [HttpGet]
        [Authorize(Policy = "BrokerRead")]
        [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<OrderDto>>> List(
            [FromQuery] int page = 1,
            [FromQuery, Range(1, 200)] int pageSize = 50,
            [FromQuery] string? q = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            CancellationToken ct = default)
        {
            var result = await _orders.ListAsync(page, pageSize, q, from, to, ct);
            return Ok(result);
        }

        /// <summary>
        /// Returns a single order by its numeric ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize(Policy = "BrokerRead")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetById([FromRoute] int id, CancellationToken ct)
        {
            var dto = await _orders.GetAsync(id, ct);
            if (dto is null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// Imports orders from a CSV file. See required headers in service.
        /// </summary>
        [HttpPost("import")]
        [Authorize(Policy = "BrokerWrite")]
        [RequestSizeLimit(50_000_000)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ImportResultDto>> Import([FromForm] IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest("file is required");

            await using var stream = file.OpenReadStream();
            var result = await _orders.ImportCsvAsync(stream, ct);
            return Ok(result);
        }

        /// <summary>
        /// Deletes an order by ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "BrokerWrite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
        {
            var ok = await _orders.DeleteAsync(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }


    }
}
