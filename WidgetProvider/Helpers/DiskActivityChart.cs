using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using WidgetProvider.Helpers.Records;

namespace WidgetProvider.Helpers;

internal sealed class DiskActivityChart : ChartBase<DiskActivitySample>
{
  public DiskActivityChart()
  {
    _readLegendUrl = CreateSolidLegendUrl();
    _writeLegendUrl = CreateDashedLegendUrl();
  }

  /// <summary>
  /// Convert KB/s value to [KMG]B/s string.
  /// </summary>
  public override string Value2String(float value) =>
    value switch
    {
      < 1024 => $"{value:F0} KB/s",
      < 1024 * 1024 => $"{value / 1024:F0} MB/s",
      _ => $"{value / (1024 * 1024):F1} GB/s",
    };

  public string ReadLegendUrl => _readLegendUrl;
  public string WriteLegendUrl => _writeLegendUrl;

  protected override float GetMaxValue(DiskActivitySample[] samples) =>
    samples.Select(s => Math.Max(s.ReadKBps, s.WriteKBps)).Max();

  protected override DiskActivitySample[] NormalizeSamples(DiskActivitySample[] samples) =>
    samples.Select(s => new DiskActivitySample(
      ReadKBps: s.ReadKBps / CurrentUpperLimitValue,
      WriteKBps: s.WriteKBps / CurrentUpperLimitValue
    )).ToArray();

  protected override string CreateChart()
  {
    ComputePoints(out var readLinePoints, out var writeLinePoints, out var readFillPoints);
    return new XElement(Ns + "svg",
      new XAttribute("height", ChartHeight),
      new XAttribute("width", ChartWidth),
      new XElement(Ns + "polyline",
        new XAttribute("points", readFillPoints),
        new XAttribute("style", $"fill:{MainColor};fill-opacity:0.3;stroke:transparent")
      ),
      new XElement(Ns + "polyline",
        new XAttribute("points", readLinePoints),
        new XAttribute("style", $"fill:none;stroke:{MainColor};stroke-width:1")
      ),
      new XElement(Ns + "polyline",
        new XAttribute("points", writeLinePoints),
        new XAttribute("style", $"fill:none;stroke:{MainColor};stroke-width:1;stroke-dasharray:2 1")
      ),
      new XElement(Ns + "rect",
        new XAttribute("height", ChartHeight),
        new XAttribute("width", ChartWidth),
        new XAttribute("style", "fill:none;stroke:rgb(106, 106, 106);stroke-width:1")
      )
    ).ToString();
  }

  protected override string MainColor => "#6DAB01";
  protected override ILogger Logger => _logger;
  protected override Dictionary<string, int> UpperLimits => SUpperLimits;

  private void ComputePoints(out string readLinePoints, out string writeLinePoints, out string readFillPoints)
  {
    var normalizedSamples = GetNormalizedSamples();
    var readPointStrings = new string[Capacity];
    var writePointStrings = new string[Capacity];
    for (var i = 0; i != Capacity; ++i)
    {
      readPointStrings[i] = $"{XCoords[i]},{(int)(ChartHeight - normalizedSamples[i].ReadKBps * ChartHeight)}";
      writePointStrings[i] = $"{XCoords[i]},{(int)(ChartHeight - normalizedSamples[i].WriteKBps * ChartHeight)}";
    }

    readLinePoints = string.Join(" ", readPointStrings);
    writeLinePoints = string.Join(" ", writePointStrings);
    readFillPoints = $"{ChartWidth},{ChartHeight} 0,{ChartHeight} " + readLinePoints;
  }

  private static readonly Dictionary<string, int> SUpperLimits = new()
  {
    { "100 KB/s", 100 },
    { "500 KB/s", 500 },
    { "1 MB/s", 1 * 1024 },
    { "10 MB/s", 10 * 1024 },
    { "100 MB/s", 100 * 1024 },
    { "250 MB/s", 200 * 1024 },
    { "500 MB/s", 500 * 1024 },
    { "1 GB/s", 1 * 1024 * 1024 },
    { "10 GB/s", 5 * 1024 * 1024 }
  };

  private readonly ILogger _logger = Utils.LoggerFactory.CreateLogger<DiskActivityChart>();
  private readonly string _readLegendUrl;
  private readonly string _writeLegendUrl;
}