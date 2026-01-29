using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace WidgetProvider.Helpers.Charts;

public abstract class ChartBase<T>
{
  public string GetChartUrl() => CreateChartUrl(CreateChart());

  public void AddSample(T sample)
  {
    lock (SamplesLock)
    {
      Samples.RemoveAt(0);
      Samples.Add(sample);
    }
  }

  public const string LegendColumnWidth = "7px";

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

  protected XElement CreateLinePointsElement(string points) => new XElement(Ns + "polyline",
    new XAttribute("points", points),
    new XAttribute("style", $"fill:none;stroke:{MainColor};stroke-width:1")
  );
  
  protected XElement CreateFillPointsElement(string points) => new XElement(Ns + "polyline",
    new XAttribute("points", points),
    new XAttribute("style", $"fill:{MainColor};fill-opacity:0.3;stroke:transparent")
  );

  protected abstract string CreateChart();
  protected abstract ILogger Logger { get; }
  protected abstract string MainColor { get; }
  protected const int Capacity = 34;
  protected const int ChartWidth = 268;
  protected const int ChartHeight = 160;
  protected static readonly XNamespace Ns = "http://www.w3.org/2000/svg";
  protected static readonly int[] XCoords = ComputeXCoords();
  protected readonly Lock SamplesLock = new();
  protected readonly List<T> Samples = [.. new T[Capacity]];

  protected static readonly XElement ChartBorder = new XElement(Ns + "rect",
    new XAttribute("height", ChartHeight),
    new XAttribute("width", ChartWidth),
    new XAttribute("style", "fill:none;stroke:rgb(106, 106, 106);stroke-width:1")
  );

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
  private const int LegendHeight = 45;
  private const int LegendWidth = 2;
}