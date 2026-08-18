// Data/Interceptors/TimestampInterceptor.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SprintBoard.Api.Models;

namespace SprintBoard.Api.Data.Interceptors;

public class TimestampInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (eventData.Context is not null)
        {
            foreach (var entry in eventData.Context.ChangeTracker.Entries<User>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SavingChangesAsync(eventData, result, ct);
    }
}