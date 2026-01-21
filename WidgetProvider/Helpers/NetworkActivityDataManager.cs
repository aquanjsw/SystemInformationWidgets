using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WidgetProvider.Helpers.Records;

namespace WidgetProvider.Helpers;

internal sealed partial class NetworkActivityDataManager : DataManagerBase<NetworkActivityChart>
{
  public NetworkActivityDataManager()
  {
    var performanceCounterCategory = new PerformanceCounterCategory("Network Interface");
    foreach (var name in performanceCounterCategory.GetInstanceNames())
    {
      _logger.LogInformation("Found network interface: {Name}", name);
      _sentSpeedCounters.Add(new PerformanceCounter("Network Interface", "Bytes Sent/sec", name));
      _recvSpeedCounters.Add(new PerformanceCounter("Network Interface", "Bytes Received/sec", name));
    }
  }

  public override Dictionary<string, string> GetData()
  {
    var sentKbps = GetCurrentTotalSentKBps() * 8 / 1024;
    var recvKbps = GetCurrentTotalRecvKBps() * 8 / 1024;
    var chartUrl = string.Empty;
    var upperLimit = string.Empty;
    if (IsChartEnabled)
    {
      Chart.AddSample(new NetworkActivitySample(sentKbps, recvKbps));
      _logger.LogDebug("Getting chart url");
      chartUrl = Chart.GetChartUrl();
      upperLimit = Chart.CurrentUpperLimitString;
    }

    return new Dictionary<string, string>
    {
      { "SentSpeed", Chart.Value2String(sentKbps) },
      { "RecvSpeed", Chart.Value2String(recvKbps) },
      { "ChartUrl", chartUrl },
      { "UpperLimit", upperLimit },
      { "SentLegendUrl", Chart.SentLegendUrl },
      { "RecvLegendUrl", Chart.RecvLegendUrl },
      { "LegendWidth", NetworkActivityChart.LegendColumnWidth }
    };
  }

  protected override void DisposeManagedResources()
  {
    for (var i = 0; i < _sentSpeedCounters.Count; i++)
    {
      _sentSpeedCounters[i].Dispose();
      _recvSpeedCounters[i].Dispose();
    }
  }

  private float GetCurrentTotalSentKBps() =>
    _sentSpeedCounters.Aggregate(0f, (acc, counter) => acc + counter.NextValue());

  private float GetCurrentTotalRecvKBps() =>
    _recvSpeedCounters.Aggregate(0f, (acc, counter) => acc + counter.NextValue());

  private readonly List<PerformanceCounter> _sentSpeedCounters = [];
  private readonly List<PerformanceCounter> _recvSpeedCounters = [];
  private readonly ILogger _logger = Utils.LoggerFactory.CreateLogger<NetworkActivityDataManager>();
}