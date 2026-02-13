namespace MechanicShop.Domain.Common;

public abstract class Entity
{
  protected Entity(){}
  protected Entity(Guid id)
  {
    Id = id == Guid.Empty ? Guid.NewGuid() : id;
  }

  public void AddDomainEvent(DomainEvent domainEvent)
    => _domainEvents.Add(domainEvent);

  public void RemoveDomainEvent(DomainEvent domainEvent)
    =>  _domainEvents.Remove(domainEvent);

  public void ClearDomainEvents()
    => _domainEvents.Clear();

  public Guid Id {get;}
  private List<DomainEvent> _domainEvents = [];
}