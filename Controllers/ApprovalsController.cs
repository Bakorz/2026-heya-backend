using CampusRooms.Api.DTOs.Approvals;
using CampusRooms.Api.Models;
using CampusRooms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusRooms.Api.Controllers;

[ApiController]
[Route("api/approvals")]
public class ApprovalsController(IApprovalService approvalService) : ControllerBase
{
    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue([FromQuery] RequestStatus status = RequestStatus.Submitted)
    {
        var queue = await approvalService.GetQueueAsync(status);
        return Ok(queue);
    }

    [HttpPost("{requestId:guid}/decide")]
    public async Task<IActionResult> Decide(Guid requestId, [FromBody] DecideRequestDto request)
    {
        var result = await approvalService.DecideAsync(requestId, request);
        if (result.NotFound)
        {
            return NotFound();
        }

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
