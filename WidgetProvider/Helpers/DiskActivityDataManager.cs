using System.Diagnostics;

namespace WidgetProvider.Helpers;

internal partial class DiskActivityDataManager : IDisposable
{
  private readonly PerformanceCounter readSpeedCounter;
  private readonly PerformanceCounter writeSpeedCounter;
  public DiskActivityDataManager()
  {
    readSpeedCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
    writeSpeedCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
  }
  private static string GetSpeedString(float speed)
  {
    if (speed < 1024 * 1024)
    {
      return $"{(speed / 1024):F0} KB/s";
    }
    else if (speed < 1024 * 1024 * 1024)
    {
      return $"{(speed / (1024 * 1024)):F1} MB/s";
    }
    return $"{(speed / (1024 * 1024 * 1024)):F1} GB/s";
  }
  public string GetReadSpeed() => GetSpeedString(readSpeedCounter.NextValue());
  public string GetWriteSpeed() => GetSpeedString(writeSpeedCounter.NextValue());
  public void Dispose()
  {
    readSpeedCounter.Dispose();
    writeSpeedCounter.Dispose();
  }
}
