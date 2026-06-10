using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Services;

namespace SprintBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        var user = await _userService.CreateFromEmailAsync(dto);
        return CreatedAtAction(nameof(Register), new { id = user.Id }, user);

    }
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> FindById(int id)
  {
    var user = await _userService.GetByIdAsync(id);
    return user is null ? NotFound(): Ok(user);
  }
}