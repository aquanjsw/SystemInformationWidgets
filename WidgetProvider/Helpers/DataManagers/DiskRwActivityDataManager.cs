using System.Diagnostics;
using WidgetProvider.Helpers.Charts;
using WidgetProvider.Helpers.Records;

namespace WidgetProvider.Helpers.DataManagers;

internal sealed partial class DiskRwActivityDataManager : DataManagerBase<DiskRwActivityChart>
{
  public override Dictionary<string, string> GetData()
  {
    var readKBps = _readBpsCounter.NextValue() / 1024;
    var writeKBps = _writeBpsCounter.NextValue() / 1024;
    var chartUrl = string.Empty;
    var upperLimit = string.Empty;
    if (IsChartEnabled)
    {
      Chart.AddSample(new DiskRwActivitySample(readKBps, writeKBps));
      chartUrl = Chart.GetChartUrl();
      upperLimit = Chart.CurrentUpperLimitString;
    }

    return new Dictionary<string, string>()
    {
      { "ReadSpeed", Value2String(readKBps) },
      { "WriteSpeed", Value2String(writeKBps) },
      { "ChartUrl", chartUrl },
      { "UpperLimit", upperLimit },
      { "ReadLegendUrl", Chart.ReadLegendUrl },
      { "WriteLegendUrl", Chart.WriteLegendUrl },
      { "LegendWidth", DiskRwActivityChart.LegendColumnWidth }
    };
  }

  protected override void DisposeManagedResources()
  {
    _readBpsCounter.Dispose();
    _writeBpsCounter.Dispose();
  }

  /// <summary>
  /// Convert KB/s value to [KMG]B/s string.
  /// </summary>
  private static string Value2String(float value) =>
    value switch
    {
      < 1024 => $"{value:F0} KB/s",
      < 1024 * 1024 => $"{value / 1024:F0} MB/s",
      _ => $"{value / (1024 * 1024):F1} GB/s",
    };

  private readonly PerformanceCounter _readBpsCounter = new("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
  private readonly PerformanceCounter _writeBpsCounter = new("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
}