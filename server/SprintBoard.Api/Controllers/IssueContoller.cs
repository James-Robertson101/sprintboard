using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.Services;
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
}