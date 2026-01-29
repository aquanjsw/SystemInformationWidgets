using System.Diagnostics;
using WidgetProvider.Helpers.Charts;

namespace WidgetProvider.Helpers.DataManagers;

public partial class DiskTimeActivityDataManager : DataManagerBase<DiskTimeActivityChart>
{
  public override Dictionary<string, string> GetData()
  {
    var chartUrl = "";
    var activeTime = _diskTimeCounter.NextValue();
    var avgTransferTime = _avgTransferTimeCounter.NextValue() * 1000; // in ms
    if (IsChartEnabled)
    {
      Chart.AddSample(activeTime / 100);
      chartUrl = Chart.GetChartUrl();
    }

    return new Dictionary<string, string>()
    {
      { "ChartUrl", chartUrl },
      { "ActiveTime", $"{activeTime:F0}%" },
      { "AvgTransferTime", Time2String(avgTransferTime) },
    };
  }

  protected override void DisposeManagedResources()
  {
    _diskTimeCounter.Dispose();
    _avgTransferTimeCounter.Dispose();
  }

  private static string Time2String(float ms) =>
    ms switch
    {
      < 0.1f => $"{ms:F0} ms",
      < 100f => $"{ms:F1} ms",
      _ => $"{ms:F0} s",
    };

  private readonly PerformanceCounter _diskTimeCounter = new("PhysicalDisk", "% Disk Time", "_Total");
  private readonly PerformanceCounter _avgTransferTimeCounter = new("PhysicalDisk", "Avg. Disk sec/Transfer", "_Total");
}