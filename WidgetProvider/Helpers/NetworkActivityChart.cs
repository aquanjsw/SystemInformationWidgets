using Microsoft.Extensions.Logging;

namespace WidgetProvider.Helpers;

internal class NetworkActivityChart : AbsoluteValuedChartBase
{
  /// <summary>
  /// Upper limits in Kbps.
  /// <para>
  /// Values are choosen based on the task manager's network graph of Windows 10/11.
  /// </para>
  /// </summary>
  protected override Dictionary<string, int> UpperLimits => new()
  {
    { "100 Kbps", 100 },
    { "500 Kbps", 500 },
    {   "1 Mbps", 1 * 1024 },
    {   "5 Mbps", 5 * 1024 },
    {  "11 Mbps", 11 * 1024 },
    {  "26 Mbps", 26 * 1024 },
    {  "54 Mbps", 54 * 1024 },
    { "100 Mbps", 100 * 1024 },
    { "250 Mbps", 250 * 1024 },
    { "500 Mbps", 500 * 1024 },
    {   "1 Gbps", 1 * 1024 * 1024 },
    {   "5 Gbps", 5 * 1024 * 1024 },
    {  "10 Gbps", 10 * 1024 * 1024 }
  };
  /// <summary>
  /// May be the minimum ratio of the adjacent upper limits.
  /// </summary>
  protected override float DecayThreshold => 0.2f;
  private readonly ILogger logger = Providers.GetLoggerFactory().CreateLogger<NetworkActivityChart>();
  protected override ILogger Logger => logger;
  public NetworkActivityChart() : base() { }
  /// <summary>
  /// Convert Kbps value to [KMG]bps string.
  /// </summary>
  public override string Value2String(float value)
  {
    string unit;
    if (value < 1024)
    {
      unit = "Kbps";
      return $"{value:F0} {unit}";
    }
    else if (value < 1024 * 1024)
    {
      unit = "Mbps";
      value /= 1024;
      return $"{value:F1} {unit}";
    }
    else
    {
      unit = "Gbps";
      value /= 1024 * 1024;
      return $"{value:F1} {unit}";
    }
  }
}

