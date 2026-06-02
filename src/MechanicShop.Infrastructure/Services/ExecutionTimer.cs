using System.Diagnostics;
using MechanicShop.Application.Common.Interfaces;

namespace MechanicShop.Infrastructure.Services;

public class ExecutionTimer : IExecutionTimer
{
  private readonly Stopwatch _stopWatch = new();
  public void Start() => _stopWatch.Start();
  public void Stop() => _stopWatch.Stop();
  public long ElapsedMilliseconds => _stopWatch.ElapsedMilliseconds;
}