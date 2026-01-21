using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace WidgetProvider.Helpers;

public abstract class ChartBase<T>
{
  public abstract string Value2String(float value);
  public string CurrentUpperLimitString => UpperLimits.Keys.ElementAt(_currentUpperLimitIndex);
  public string GetChartUrl() => CreateChartUrl(CreateChart());

  public void AddSample(T sample)
  {
    lock (_samplesLock)
    {
      _samples.RemoveAt(0);
      _samples.Add(sample);
    }
  }

  public const string LegendColumnWidth = "7px";

  protected T[] GetNormalizedSamples()
  {
    T[] samplesCopy;
    lock (_samplesLock)
    {
      samplesCopy = _samples.ToArray();
    }

    var max = GetMaxValue(samplesCopy);
    var currentUpperLimit = UpperLimits.Values.ElementAt(_currentUpperLimitIndex);
    if (max > currentUpperLimit)
    {
      if (_currentUpperLimitIndex < UpperLimits.Count - 1)
      {
        _currentUpperLimitIndex++;
        Logger.LogInformation("Increased upper limit to {UpperLimit}",
          CurrentUpperLimitString);
      }
    }
    else if (_currentUpperLimitIndex != 0 && max < UpperLimits.Values.ElementAt(_currentUpperLimitIndex - 1))
    {
      _currentUpperLimitIndex--;
      Logger.LogInformation("Decreased upper limit to {UpperLimit}",
        CurrentUpperLimitString);
    }

    return NormalizeSamples(samplesCopy);
  }

  protected string CreateSolidLegendUrl() => CreateChartUrl(new XElement(Ns + "svg",
    new XAttribute("height", LegendHeight),
    new XAttribute("width", LegendWidth),
    new XElement(Ns + "polyline",
      new XAttribute("points", $"0,0 0,{LegendHeight}"),
      new XAttribute("style", $"fill:none;stroke:{MainColor};stroke-width:{LegendStrokeWidth}"))
  ).ToString());

  protected string CreateDashedLegendUrl() => CreateChartUrl(new XElement(Ns + "svg",
    new XAttribute("height", LegendHeight),
    new XAttribute("width", LegendWidth),
    new XElement(Ns + "polyline",
      new XAttribute("points", $"0,0 0,{LegendHeight}"),
      new XAttribute("style", $"fill:none;stroke:{MainColor};stroke-width:{LegendStrokeWidth};stroke-dasharray:2 2"))
  ).ToString());

  protected abstract string CreateChart();
  protected abstract float GetMaxValue(T[] samples);
  protected abstract T[] NormalizeSamples(T[] samples);
  protected abstract Dictionary<string, int> UpperLimits { get; }
  protected abstract ILogger Logger { get; }
  protected abstract string MainColor { get; }
  protected int CurrentUpperLimitValue => UpperLimits.Values.ElementAt(_currentUpperLimitIndex);
  protected const int Capacity = 34;
  protected const int ChartWidth = 268;
  protected const int ChartHeight = 150;
  protected static readonly XNamespace Ns = "http://www.w3.org/2000/svg";
  protected static readonly int[] XCoords = ComputeXCoords();

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
    var xCoords = steps.Select(x => acc += x).Prepend(0).ToArray();
    // For beautification
    xCoords[Capacity - 1] += 1;
    return xCoords;
  }

  private static string CreateChartUrl(string svg) =>
    "data:image/svg+xml;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(svg));

  private const int LegendStrokeWidth = 3;
  private const int LegendHeight = 55;
  private const int LegendWidth = 2;
  private int _currentUpperLimitIndex;
  private readonly Lock _samplesLock = new();
  private readonly List<T> _samples = [.. new T[Capacity]];
}