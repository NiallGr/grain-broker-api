using GrainBroker.Core.DTOs;
using GrainBroker.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrainBroker.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orders;

        public OrdersController(IOrderService orders) => _orders = orders;

        [HttpGet]
        [Authorize(Policy = "BrokerRead")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll(CancellationToken ct)
        {
            var result = await _orders.ListAsync(1, 100, null, null, null, ct);
            return Ok(result.Items); 
        }


        [HttpPost("import")]
        [Authorize(Policy = "BrokerWrite")]
        [RequestSizeLimit(50_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImportResultDto>> Import([FromForm] IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest("file is required");

            await using var stream = file.OpenReadStream();
            var result = await _orders.ImportCsvAsync(stream, ct);

            return Ok(result);
        }


    }
}
