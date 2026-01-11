using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace WidgetProvider.Helpers;

internal partial class NetworkActivityDataManager : IDisposable
{
  private readonly List<PerformanceCounter> sentSpeedCounters = [];
  private readonly List<PerformanceCounter> recvSpeedCounters = [];
  private readonly ILogger logger = Providers.GetLoggerFactory().CreateLogger<NetworkActivityDataManager>();
  private readonly NetworkActivityChart chart = new();
  public bool IsChartEnabled { get; set; } = false;
  public NetworkActivityDataManager()
  {
    var pcc = new PerformanceCounterCategory("Network Interface");
    foreach (var name in pcc.GetInstanceNames())
    {
      logger.LogInformation("Found network interface: {Name}", name);
      sentSpeedCounters.Add(new PerformanceCounter("Network Interface", "Bytes Sent/sec", name));
      recvSpeedCounters.Add(new PerformanceCounter("Network Interface", "Bytes Received/sec", name));
    }
  }
  private float GetSentSpeed() => sentSpeedCounters.Aggregate(0f, (acc, counter) => acc + counter.NextValue());
  private float GetRecvSpeed() => recvSpeedCounters.Aggregate(0f, (acc, counter) => acc + counter.NextValue());
  public NetworkActivityData GetData()
  {
    var sentSpeed = GetSentSpeed() * 8 / 1024;
    var recvSpeed = GetRecvSpeed() * 8 / 1024;
    var chartURL = string.Empty;
    var upperLimit = string.Empty;
    if (IsChartEnabled)
    {
      chart.AddValue(sentSpeed + recvSpeed);
      chartURL = chart.CreateURL();
      upperLimit = chart.CurrentUpperLimitString;
    }
    var ret = new NetworkActivityData(
      SentSpeed: chart.Value2String(sentSpeed),
      RecvSpeed: chart.Value2String(recvSpeed),
      ChartURL: chartURL,
      UpperLimit: upperLimit
    );
    return ret;
  }
  public void Dispose()
  {
    for (int i = 0; i < sentSpeedCounters.Count; i++)
    {
      sentSpeedCounters[i].Dispose();
      recvSpeedCounters[i].Dispose();
    }
  }
}

internal record NetworkActivityData(
  string SentSpeed,
  string RecvSpeed,
  string ChartURL,
  string UpperLimit
);

