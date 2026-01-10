using Microsoft.Extensions.Logging;

namespace WidgetProvider.Helpers;

internal class NetworkActivityChart : AbsoluteValuedChartBase
{
  /// <summary>
  /// Upper limits in Kbps.
  /// <para>
  /// The values are from by observing the network
  /// graph in Windows Task Manager.
  /// </para>
  /// </summary>
  protected override int[] UpperLimits => [
    100, // 100 Kbps
    500, // 500 Kbps
    1 * 1024, // 1 Mbps
    11 * 1024, // 11 Mbps
    26 * 1024, // 26 Mbps
    54 * 1024, // 54 Mbps
    100 * 1024, // 100 Mbps
    250 * 1024, // 250 Mbps
    500 * 1024, // 500 Mbps
    1 * 1024 * 1024, // 1 Gbps
    10 * 1024 * 1024 // 10 Gbps
  ];
  private readonly ILogger logger = Providers.GetLoggerFactory().CreateLogger<NetworkActivityChart>();
  protected override ILogger Logger => logger;
  public NetworkActivityChart() : base() { }
  /// <summary>
  /// Convert Kbps value to [KMG]bps string.
  /// </summary>
  public override string Value2String(float value, int precision)
  {
    string unit;
    if (value < 1024)
    {
      unit = "Kbps";
    }
    else if (value < 1024 * 1024)
    {
      unit = "Mbps";
      value /= 1024;
    }
    else
    {
      unit = "Gbps";
      value /= 1024 * 1024;
    }
    return value.ToString($"F{precision}") + " " + unit;
  }
}

