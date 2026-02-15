using CampusRooms.Api.DTOs.Requests;
using CampusRooms.Api.Models;
using CampusRooms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusRooms.Api.Controllers;

[ApiController]
[Route("api/requests")]
public class RequestsController(IRequestService requestService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRequests(
        [FromQuery] string? requestedBy,
        [FromQuery] RequestStatus? status,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool desc = true)
    {
        var requests = await requestService.GetRequestsAsync(requestedBy, status, search, sortBy, desc);
        return Ok(requests);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRequest([FromBody] CreateBookingRequestDto request)
    {
        var result = await requestService.CreateRequestAsync(request);
        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Created($"/api/requests/{result.Value!.Id}", result.Value);
    }
}
