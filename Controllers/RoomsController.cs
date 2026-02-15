using CampusRooms.Api.DTOs.Rooms;
using CampusRooms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusRooms.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(IRoomService roomService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRooms([FromQuery] string? search, [FromQuery] bool? isActive)
    {
        var rooms = await roomService.GetRoomsAsync(search, isActive);
        return Ok(rooms);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequestDto request)
    {
        var result = await roomService.CreateRoomAsync(request);
        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Created($"/api/rooms/{result.Value!.Id}", result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomRequestDto request)
    {
        var result = await roomService.UpdateRoomAsync(id, request);
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

    [HttpPatch("{id:guid}/active")]
    public async Task<IActionResult> SetRoomActive(Guid id, [FromBody] SetRoomActiveRequestDto request)
    {
        var result = await roomService.SetActiveAsync(id, request);
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
