namespace MechanicShop.Application.Common.Interfaces;

public interface IExecutionTimer
{
  void Start();

  void Stop();

  public long ElapsedMilliseconds { get; }
}