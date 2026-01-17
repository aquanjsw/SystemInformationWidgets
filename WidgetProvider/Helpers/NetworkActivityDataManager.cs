using Microsoft.Extensions.Logging;
using System.Diagnostics;
using WidgetProvider.Helpers.Records;

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

  public Dictionary<string, string> GetData()
  {
    var sentKbps = GetSentSpeed() * 8 / 1024;
    var recvKbps = GetRecvSpeed() * 8 / 1024;
    var chartUrl = string.Empty;
    var upperLimit = string.Empty;
    if (IsChartEnabled)
    {
      chart.AddSample(new NetworkActivitySample(sentKbps, recvKbps));
      logger.LogDebug("Getting chart url");
      chartUrl = chart.GetMainChartUrl();
      upperLimit = chart.CurrentUpperLimitString;
    }

    return new Dictionary<string, string>
    {
      { "SentSpeed", NetworkActivityChart.Value2String(sentKbps) },
      { "RecvSpeed", NetworkActivityChart.Value2String(recvKbps) },
      { "ChartUrl", chartUrl },
      { "UpperLimit", upperLimit },
      { "SentLegendUrl", NetworkActivityChart.SentLegendUrl },
      { "RecvLegendUrl", NetworkActivityChart.RecvLegendUrl }
    };
  }

  public void Dispose()
  {
    for (var i = 0; i < sentSpeedCounters.Count; i++)
    {
      sentSpeedCounters[i].Dispose();
      recvSpeedCounters[i].Dispose();
    }
  }
}