using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (claim is null || !int.TryParse(claim, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId;
    }
}