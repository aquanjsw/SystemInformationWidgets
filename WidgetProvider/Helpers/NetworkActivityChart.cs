using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using WidgetProvider.Helpers.Records;

namespace WidgetProvider.Helpers;

internal class NetworkActivityChart
{
  /// <summary>
  /// Upper limits in Kbps.
  /// <para>
  /// Values are chosen based on the task manager's network graph of Windows 11.
  /// </para>
  /// </summary>
  private static Dictionary<string, int> UpperLimits => new()
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
    { "5 Gbps", 5 * 1024 * 1024 },
    { "10 Gbps", 10 * 1024 * 1024 }
  };

  /// <summary>
  /// Maybe the minimum ratio of the adjacent upper limits.
  /// </summary>
  private static float DecayThreshold => 0.2f;

  private const int ChartWidth = 268;
  private const int ChartHeight = 150;
  private const int Capacity = 34;
  private const string MainColor = "rgb(245,98,142)";
  private const int LegendStrokeWidth = 3;
  private const int LegendHeight = 55;
  private const int LegendWidth = 2;
  private static readonly XNamespace Ns = "http://www.w3.org/2000/svg";

  private readonly ILogger _logger = Providers.GetLoggerFactory().CreateLogger<NetworkActivityChart>();
  private readonly List<NetworkActivitySample> _samples = [.. new NetworkActivitySample[Capacity]];
  private readonly Lock _samplesLock = new();
  private static readonly int[] XCoords = ComputeXCoords();
  private int _currentUpperLimitIndex = 0;
  public string CurrentUpperLimitString => UpperLimits.Keys.ElementAt(_currentUpperLimitIndex);
  private int CurrentUpperLimitValue => UpperLimits.Values.ElementAt(_currentUpperLimitIndex);

  public static readonly string RecvLegendUrl = GetChartUrl(new XElement(Ns + "svg",
    new XAttribute("height", LegendHeight),
    new XAttribute("width", LegendWidth),
    new XElement(Ns + "polyline",
      new XAttribute("points", $"0,0 0,{LegendHeight}"),
      new XAttribute("style", $"fill:none;stroke:{MainColor};stroke-width:{LegendStrokeWidth}"))
  ).ToString());

  public static readonly string SentLegendUrl = GetChartUrl(new XElement(Ns + "svg",
    new XAttribute("height", LegendHeight),
    new XAttribute("width", LegendWidth),
    new XElement(Ns + "polyline",
      new XAttribute("points", $"0,0 0,{LegendHeight}"),
      new XAttribute("style",
        $"fill:none;stroke:{MainColor};stroke-width:{LegendStrokeWidth};stroke-dasharray:2 2"))
  ).ToString());

  public string GetMainChartUrl() => GetChartUrl(CreateMainChart());

  private static string GetChartUrl(string svg) =>
    "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));

  private string CreateMainChart()
  {
    _logger.LogDebug("Entering CreateMainChart");
    ComputePoints(out var sentLinePoints, out var recvLinePoints, out var recvFillPoints);
    var ret = new XElement(Ns + "svg",
      new XAttribute("height", ChartHeight),
      new XAttribute("width", ChartWidth),
      new XElement(Ns + "defs",
        new XElement(Ns + "linearGradient",
          new XAttribute("x1", "0%"),
          new XAttribute("x2", "0%"),
          new XAttribute("y1", "0%"),
          new XAttribute("y2", "100%"),
          new XAttribute("id", "gradientId"),
          new XElement(Ns + "stop",
            new XAttribute("offset", "0%"),
            new XAttribute("style", $"stop-color:{MainColor};stop-opacity:0.4")
          ),
          new XElement(Ns + "stop",
            new XAttribute("offset", "95%"),
            new XAttribute("style", "stop-color:rgb(130,0,47);stop-opacity:0.25")
          )
        )
      ),
      new XElement(Ns + "polyline",
        new XAttribute("points", recvFillPoints),
        new XAttribute("style", "fill:url(#gradientId);stroke:transparent")
      ),
      new XElement(Ns + "polyline",
        new XAttribute("points", recvLinePoints),
        new XAttribute("style", $"fill:none;stroke:{MainColor};stroke-width:1")
      ),
      new XElement(Ns + "polyline",
        new XAttribute("points", sentLinePoints),
        new XAttribute("style", $"fill:none;stroke:{MainColor};stroke-width:1;stroke-dasharray:2 1")
      ),
      new XElement(Ns + "rect",
        new XAttribute("height", ChartHeight),
        new XAttribute("width", ChartWidth),
        new XAttribute("style", "fill:none;stroke:rgb(106, 106, 106);stroke-width:1")
      )
    ).ToString();
    return ret;
  }

  private void ComputePoints(out string sentLinePoints, out string recvLinePoints, out string recvFillPoints)
  {
    var normalizedSamples = NormalizeSamples();

    List<string> sentPointStrings = [];
    List<string> recvPointStrings = [];
    for (var i = 0; i != Capacity; ++i)
    {
      sentPointStrings.Add($"{XCoords[i]},{(int)(ChartHeight - (normalizedSamples[i].SentKbps * ChartHeight))}");
      recvPointStrings.Add($"{XCoords[i]},{(int)(ChartHeight - (normalizedSamples[i].RecvKbps * ChartHeight))}");
    }

    sentLinePoints = string.Join(" ", sentPointStrings);
    recvLinePoints = string.Join(" ", recvPointStrings);
    recvFillPoints = $"{ChartWidth},{ChartHeight} 0,{ChartHeight} {recvLinePoints}";
  }

  /// <summary>
  /// Convert Kbps value to [KMG]bps string.
  /// </summary>
  public static string Value2String(float kbps) =>
    kbps switch
    {
      < 1024 => $"{kbps:F0} Kbps",
      < 1024 * 1024 => $"{(kbps / 1024):F1} Mbps",
      _ => $"{(kbps / (1024 * 1024)):F1} Gbps",
    };

  public void AddSample(NetworkActivitySample sample)
  {
    lock (_samplesLock)
    {
      _samples.RemoveAt(0);
      _samples.Add(sample);
    }
  }

  private NetworkActivitySample[] NormalizeSamples()
  {
    NetworkActivitySample[] samples;
    lock (_samplesLock)
    {
      samples = _samples.ToArray();
    }

    var max = samples.Select(s => Math.Max(s.RecvKbps, s.SentKbps)).Max();
    if (max > CurrentUpperLimitValue)
    {
      if (_currentUpperLimitIndex < UpperLimits.Count - 1)
      {
        _currentUpperLimitIndex++;
        _logger.LogInformation("Increased upper limit to {UpperLimit}",
          CurrentUpperLimitString);
      }
    }
    else if (max < CurrentUpperLimitValue * DecayThreshold)
    {
      if (_currentUpperLimitIndex != 0)
      {
        _currentUpperLimitIndex--;
        _logger.LogInformation("Decreased upper limit to {UpperLimit}",
          CurrentUpperLimitString);
      }
    }

    var ret = samples.Select(s =>
      new NetworkActivitySample(s.SentKbps / CurrentUpperLimitValue, s.RecvKbps / CurrentUpperLimitValue)).ToArray();
    return ret;
  }

  private static int[] ComputeXCoords()
  {
    const int initStep = (ChartWidth - 1) / (Capacity - 1);
    const int nRest = (ChartWidth - 1) % (Capacity - 1);
    var steps = new int[Capacity - 1];
    Array.Fill(steps, initStep);
    switch (nRest)
    {
      case 1:
        steps[0] += 1;
        break;
      case > 1:
      {
        const int restStep = (Capacity - 1) / (nRest - 1);
        for (var i = 0; i < nRest; i++)
        {
          steps[i * restStep] += 1;
        }

        break;
      }
    }

    var acc = 0;
    var ret = steps.Select(x => acc += x).Prepend(0).ToArray();
    // For beautification
    ret[Capacity - 1] += 1;
    return ret;
  }
}