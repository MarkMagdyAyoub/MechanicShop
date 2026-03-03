using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common;
using MechanicShop.Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MechanicShop.Infrastructure.Data.Interceptors;

public sealed class UpdatedEntitiesInterceptor(
  TimeProvider timeProvider,
  IUser user
) : SaveChangesInterceptor
{
  private readonly TimeProvider _timeProvider = timeProvider;
  private readonly IUser _user = user;

  public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
  {
    UpdatedEntitiesAuditingTimes(eventData.Context);
    return base.SavedChanges(eventData, result);
  }

  public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
  {
    UpdatedEntitiesAuditingTimes(eventData.Context);
    return base.SavedChangesAsync(eventData, result, cancellationToken);
  }

  public void UpdatedEntitiesAuditingTimes(DbContext? context)
  {
    if(context is null) return;

    foreach(var updatedEntity in context.ChangeTracker.Entries<AuditableEntity>())
    {
      if(
          updatedEntity.State is 
              EntityState.Modified or
              EntityState.Added ||
              updatedEntity.HasOwnEntity()
        )
      {
        var utcNow = _timeProvider.GetUtcNow();

        if(updatedEntity.State is EntityState.Added)
        {
          updatedEntity.Entity.CreatedAtUtc = utcNow;
          updatedEntity.Entity.CreatedBy = _user.Id.ToString();
        }

        updatedEntity.Entity.LastModifiedUtc = utcNow;
        updatedEntity.Entity.LastModifiedBy = _user.Id.ToString();

        foreach(var ownedEntity in updatedEntity.References)
        {
          if(
            ownedEntity.TargetEntry?.State is 
                EntityState.Modified or 
                EntityState.Added && 
                ownedEntity.TargetEntry.Entity is AuditableEntity auditableEntity
          )
          {
            if(ownedEntity.TargetEntry.State is EntityState.Added)
            {
              auditableEntity.CreatedAtUtc = utcNow;
              auditableEntity.CreatedBy = _user.Id.ToString();
            }

            auditableEntity.LastModifiedUtc = utcNow;
            auditableEntity.LastModifiedBy = _user.Id.ToString();
          }
        }
      }
    }
  }
}