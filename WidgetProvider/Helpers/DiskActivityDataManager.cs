using System.Diagnostics;
using WidgetProvider.Helpers.Records;

namespace WidgetProvider.Helpers;

internal sealed partial class DiskActivityDataManager : DataManagerBase<DiskActivityChart>
{
  public override Dictionary<string, string> GetData()
  {
    var readKBps = _readBpsCounter.NextValue() / 1024;
    var writeKBps = _writeBpsCounter.NextValue() / 1024;
    var chartUrl = string.Empty;
    var upperLimit = string.Empty;
    if (IsChartEnabled)
    {
      Chart.AddSample(new DiskActivitySample(readKBps, writeKBps));
      chartUrl = Chart.GetChartUrl();
      upperLimit = Chart.CurrentUpperLimitString;
    }

    return new Dictionary<string, string>()
    {
      { "ReadSpeed", Chart.Value2String(readKBps) },
      { "WriteSpeed", Chart.Value2String(writeKBps) },
      { "ChartUrl", chartUrl },
      { "UpperLimit", upperLimit },
      { "ReadLegendUrl", Chart.ReadLegendUrl },
      { "WriteLegendUrl", Chart.WriteLegendUrl },
      { "LegendWidth", DiskActivityChart.LegendColumnWidth }
    };
  }

  protected override void DisposeManagedResources()
  {
    _readBpsCounter.Dispose();
    _writeBpsCounter.Dispose();
  }

  private readonly PerformanceCounter _readBpsCounter = new("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
  private readonly PerformanceCounter _writeBpsCounter = new("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
}