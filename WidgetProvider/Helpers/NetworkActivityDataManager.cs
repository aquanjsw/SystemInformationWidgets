using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace WidgetProvider.Helpers;

internal partial class NetworkActivityDataManager : IDisposable
{
  private readonly List<PerformanceCounter> sentSpeedCounters = [];
  private readonly List<PerformanceCounter> recvSpeedCounters = [];
  private readonly ILogger logger;
  public NetworkActivityDataManager()
  {
    logger = Providers.GetLoggerFactory().CreateLogger<NetworkActivityDataManager>();
    var pcc = new PerformanceCounterCategory("Network Interface");
    foreach (var name in pcc.GetInstanceNames())
    {
      logger.LogInformation("Found network interface: {Name}", name);
      sentSpeedCounters.Add(new PerformanceCounter("Network Interface", "Bytes Sent/sec", name));
      recvSpeedCounters.Add(new PerformanceCounter("Network Interface", "Bytes Received/sec", name));
    }
  }
  public string GetSentSpeed() => GetSpeedString(sentSpeedCounters.Aggregate(0f, (acc, counter) => acc + counter.NextValue()));
  public string GetRecvSpeed() => GetSpeedString(recvSpeedCounters.Aggregate(0f, (acc, counter) => acc + counter.NextValue()));
  public void Dispose()
  {
    for (int i = 0; i < sentSpeedCounters.Count; i++)
    {
      sentSpeedCounters[i].Dispose();
      recvSpeedCounters[i].Dispose();
    }
  }
  private static string GetSpeedString(float speed)
  {
    if (speed < 1024 * 1024)
    {
      return $"{speed / 1024:F0} KB/s";
    }
    return $"{speed / 1024 / 1024:F1} MB/s";
  }
}

