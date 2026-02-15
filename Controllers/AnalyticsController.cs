using CampusRooms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusRooms.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents(
        [FromQuery] string? entityType,
        [FromQuery] string? eventType,
        [FromQuery] string? actor,
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool desc = true)
    {
        var eventsData = await analyticsService.GetEventsAsync(entityType, eventType, actor, search, sortBy, desc);
        return Ok(eventsData);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await analyticsService.GetSummaryAsync();
        return Ok(summary);
    }
}
