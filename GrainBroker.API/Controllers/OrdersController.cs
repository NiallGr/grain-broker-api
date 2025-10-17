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
        private readonly IOrderAnalysisService _analysis;

        public OrdersController(IOrderService orders, IOrderAnalysisService analysis)
        {
            _analysis = analysis;
            _orders = orders;
        }


        /// <summary>
        /// Returns a paged list of orders.
        /// </summary>
        /// <param name="page">1-based page index (default 1)</param>
        /// <param name="amount">Items per page 1..200 (default 50)</param>
        [HttpGet]
        [Authorize(Policy = "BrokerRead")]
        [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<OrderDto>>> List(
            [FromQuery] int page = 1,
            [FromQuery, Range(1, 200)] int amount = 50,
            CancellationToken ct = default)
        {
            var result = await _orders.ListAsync(page, amount, ct);
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

        [HttpGet("insights")]
        [Authorize(Policy = "BrokerRead")]
        [ProducesResponseType(typeof(OrderInsightsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public async Task<ActionResult<OrderInsightsDto>> Insights(CancellationToken ct)
        {
            try
            {
                var result = await _analysis.AnalyzeLatestAsync(ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }


    }
}
