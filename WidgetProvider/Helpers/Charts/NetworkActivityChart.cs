using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using WidgetProvider.Helpers.Charts;
using WidgetProvider.Helpers.Records;

namespace WidgetProvider.Helpers;

internal sealed class NetworkActivityChart : AbsoluteValuedChartBase<NetworkActivitySample>
{
  public NetworkActivityChart()
  {
    _recvLegendUrl = CreateSolidLegendUrl();
    _sentLegendUrl = CreateDashedLegendUrl();
  }

  public string RecvLegendUrl => _recvLegendUrl;
  public string SentLegendUrl => _sentLegendUrl;

  protected override float GetMaxValue(NetworkActivitySample[] samples) =>
    samples.Select(s => Math.Max(s.SentKbps, s.RecvKbps)).Max();

  protected override string CreateChart()
  {
    ComputePoints(out var sentLinePoints, out var recvLinePoints, out var recvFillPoints);
    var ret = new XElement(Ns + "svg",
      new XAttribute("height", ChartHeight),
      new XAttribute("width", ChartWidth),
  CreateFillPointsElement(recvFillPoints),
      CreateLinePointsElement(recvLinePoints),
      CreateLinePointsElement(sentLinePoints),
      ChartBorder
    ).ToString();
    return ret;
  }

  protected override NetworkActivitySample[] NormalizeSamples(NetworkActivitySample[] samples) =>
    samples.Select(s => new NetworkActivitySample(
      SentKbps: s.SentKbps / CurrentUpperLimitValue,
      RecvKbps: s.RecvKbps / CurrentUpperLimitValue)).ToArray();

  protected override string MainColor => "#db396f";
  protected override ILogger Logger => _logger;
  protected override Dictionary<string, int> UpperLimits => SUpperLimits;

  private void ComputePoints(out string sentLinePoints, out string recvLinePoints, out string recvFillPoints)
  {
    var normalizedSamples = GetNormalizedSamples();
    var sentPointStrings = new string[Capacity];
    var recvPointStrings = new string[Capacity];
    for (var i = 0; i != Capacity; ++i)
    {
      sentPointStrings[i] = $"{XCoords[i]},{(int)(ChartHeight - normalizedSamples[i].SentKbps * ChartHeight)}";
      recvPointStrings[i] = $"{XCoords[i]},{(int)(ChartHeight - normalizedSamples[i].RecvKbps * ChartHeight)}";
    }

    sentLinePoints = string.Join(" ", sentPointStrings);
    recvLinePoints = string.Join(" ", recvPointStrings);
    recvFillPoints = $"{ChartWidth},{ChartHeight} 0,{ChartHeight} {recvLinePoints}";
  }

  /// <summary>
  /// Upper limits in Kbps.
  /// <para>
  /// Values are chosen based on the task manager's network graph of Windows 11.
  /// </para>
  /// </summary>
  private static readonly Dictionary<string, int> SUpperLimits = new()
  {
    { "100 Kbps", 100 },
    { "500 Kbps", 500 },
    { "1 Mbps", 1 * 1024 },
    { "5 Mbps", 5 * 1024 },
    { "11 Mbps", 11 * 1024 },
    { "26 Mbps", 26 * 1024 },
    { "54 Mbps", 54 * 1024 },
    { "100 Mbps", 100 * 1024 },
    { "250 Mbps", 250 * 1024 },
    { "500 Mbps", 500 * 1024 },
    { "1 Gbps", 1 * 1024 * 1024 },
  };

  private readonly ILogger _logger = Utils.LoggerFactory.CreateLogger<NetworkActivityChart>();
  private readonly string _recvLegendUrl;
  private readonly string _sentLegendUrl;
}