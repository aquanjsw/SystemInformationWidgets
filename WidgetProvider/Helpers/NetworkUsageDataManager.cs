using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;

namespace WidgetProvider.Helpers;

internal partial class NetworkUsageDataManager : IDisposable
{
  private PerformanceCounter? sentSpeedCounter;
  private PerformanceCounter? recvSpeedCounter;
  private string currentIf = "";
  private readonly ILogger logger;
  public NetworkUsageDataManager()
  {
    logger = Providers.GetLoggerFactory().CreateLogger<NetworkUsageDataManager>();
    UpdateCounters();
  }
  public string Interface => currentIf;
  public string GetSentSpeed()
  {
    if (sentSpeedCounter == null)
    {
      return "0 B/s";
    }
    var speed = sentSpeedCounter.NextValue();
    return GetSpeedString(speed);
  }
  public string GetRecvSpeed()
  {
    if (recvSpeedCounter == null)
    {
      return "0 B/s";
    }
    var speed = recvSpeedCounter.NextValue();
    return GetSpeedString(speed);
  }
  public void UpdateCounters()
  {
    var defaultIf = GetDefaultInterface();
    if (defaultIf == "")
    {
      sentSpeedCounter?.Dispose();
      recvSpeedCounter?.Dispose();
      sentSpeedCounter = null;
      recvSpeedCounter = null;
      logger.LogInformation("No outbound network interface detected.");
    }
    else if (currentIf != defaultIf)
    {
      sentSpeedCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", defaultIf);
      recvSpeedCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", defaultIf);
      currentIf = defaultIf;
      logger.LogInformation("Network interface changed to: {interface}", currentIf);
    }
  }
  public void Dispose()
  {
    sentSpeedCounter?.Dispose();
    recvSpeedCounter?.Dispose();
  }
  private static string GetSpeedString(float speed)
  {
    if (speed < 1024)
    {
      return $"{speed:F0} B/s";
    }
    else if (speed < 1024 * 1024)
    {
      return $"{speed / 1024:F0} KB/s";
    }
    return $"{speed / 1024 / 1024:F1} MB/s";
  }

  /// <summary>
  /// Get network interface description that:
  /// <list type="bullet">
  /// <item>has gateway</item>
  /// <item>with minimum metric</item>
  /// </list>
  /// </summary>
  /// <return>
  /// <c>null</c> if no outbound interface
  /// </return>
  private static string GetDefaultInterface()
  {
    var metrics = new ManagementObjectSearcher(
      "ROOT\\CIMV2",
      "SELECT InterfaceIndex, IPConnectionMetric FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE AND IPConnectionMetric != \"\" AND InterfaceIndex != \"\"")
      .Get()
      .Cast<ManagementObject>()
      .Select(x => new
      {
        Index = x["InterfaceIndex"] as uint?,
        Metric = x["IPConnectionMetric"] as uint?
      })
      .Where(x => x.Index.HasValue && x.Metric.HasValue)
      .ToDictionary(x => x.Index!.Value, x => x.Metric!.Value);

    return NetworkInterface.GetAllNetworkInterfaces()
    .Where(ni =>
      ni.OperationalStatus == OperationalStatus.Up &&
      ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
      ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
      ni.GetIPProperties().GatewayAddresses.Any(g =>
        g?.Address != null &&
        !g.Address.Equals(IPAddress.Any) &&
        !g.Address.Equals(IPAddress.IPv6Any)))
    .Where(ni =>
      ni.GetIPProperties().GetIPv4Properties() != null &&
      metrics.ContainsKey(Convert.ToUInt32(ni.GetIPProperties().GetIPv4Properties().Index)))
    .Select(ni => new
    {
      Interface = ni,
      Metric = metrics[Convert.ToUInt32(ni.GetIPProperties().GetIPv4Properties().Index)]
    })
    .OrderBy(x => x.Metric)
    .Select(x => x.Interface.Description)
    .FirstOrDefault("");
  }
}

