using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace MechanicShop.Infrastructure.Data.Extensions;

public static class Extensions{
  public static bool HasOwnEntity(this EntityEntry entry) =>
    entry.References.Any(
        r =>
            r.TargetEntry?.Metadata.IsOwned() == true &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified)
    );
}

