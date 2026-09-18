using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SprintBoard.Api.Hubs;

[Authorize]
public class SprintBoardHub : Hub
{
    public async Task JoinProject(string projectId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"project-{projectId}"
        );

        Console.WriteLine(
            $"[SignalR] {Context.ConnectionId} joined project-{projectId}"
        );
    }

    public async Task LeaveProject(string projectId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"project-{projectId}"
        );

        Console.WriteLine(
            $"[SignalR] {Context.ConnectionId} left project-{projectId}"
        );
    }
}