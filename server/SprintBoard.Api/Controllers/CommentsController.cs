// Controllers/CommentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SprintBoard.Api.DTOs;
using SprintBoard.Api.Services;

namespace SprintBoard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:int}/issues/{issueId:int}/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    private int CurrentUserId => User.GetUserId();

    [HttpGet]
    public async Task<ActionResult<List<CommentResponseDto>>> GetComments(int projectId, int issueId)
    {
        var comments = await _commentService.GetCommentsAsync(projectId, issueId, CurrentUserId);
        return Ok(comments);
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponseDto>> CreateComment(int projectId, int issueId, [FromBody] CreateCommentDto dto)
    {
        var created = await _commentService.CreateCommentAsync(projectId, issueId, CurrentUserId, dto);
        return CreatedAtAction(nameof(GetComments), new { projectId, issueId }, created);
    }

    [HttpPut("{commentId:int}")]
    public async Task<ActionResult<CommentResponseDto>> UpdateComment(int projectId, int issueId, int commentId, [FromBody] UpdateCommentDto dto)
    {
        var updated = await _commentService.UpdateCommentAsync(projectId, issueId, commentId, CurrentUserId, dto);
        return Ok(updated);
    }

    [HttpDelete("{commentId:int}")]
    public async Task<IActionResult> DeleteComment(int projectId, int issueId, int commentId)
    {
        await _commentService.DeleteCommentAsync(projectId, issueId, commentId, CurrentUserId);
        return NoContent();
    }
}