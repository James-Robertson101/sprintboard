using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Models;
using SprintBoard.Api.Services;
using Microsoft.AspNetCore.Authorization;

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

    
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserDto>> FindById(int id)
  {
    var user = await _userService.GetByIdAsync(id);
    return user is null ? NotFound(): Ok(user);
  }
}