using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.Services;
using Microsoft.AspNetCore.Authorization;
using SprintBoard.Api.DTOs;
namespace SprintBoard.Api.Controllers;



[ApiController]
[Route("api/[controller]")]
public class IssueController:ControllerBase
{
  private readonly IIssueService _service;
  public IssueController(IIssueService service)
  {
    _service = service;
  }

  // [Authorize]
  // [HttpPost("CreateIssue")]
  // public async  Task<ActionResult<IssueResponseDto>> CreateIssue(IssueDto issue)
  // {
  //           var userId = User.GetUserId();

  //           var response = await _service.CreateIssue(
  //               issue);

  //           return Ok(response);
  // }
} 
  