using Microsoft.Extensions.Logging;
using System.Text;
using System.Xml.Linq;

namespace WidgetProvider.Helpers;

/// <summary>
/// Base class for single line chart.
/// </summary>
internal abstract class ChartBase
{
  protected virtual string[] StopStyles => [
    "stop-color:rgb(245,98,142);stop-opacity:0.4",
    "stop-color:rgb(130,0,47);stop-opacity:0.25"
  ];
  protected virtual string LineStyle => "fill:none;stroke:rgb(245,98,142);stroke-width:1";
  protected virtual string FillStyle => "fill:url(#gradientId);stroke:transparent";
  protected abstract ILogger Logger { get; }
  protected int chartWidth;
  protected int chartHeight;
  protected const int capacity = 34;
  protected List<float> chartValues = [.. new float[capacity]];
  protected abstract List<float> NormalizedChartValues { get; }
  protected int[] xCoords = [];

  public ChartBase()
  {
    SetChartSize(Enums.ChartSize.Medium);
  }
  public string CreateURL()
  {
    var chartStr = CreateChart();
    chartStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(chartStr));
    var url = "data:image/svg+xml;base64," + chartStr;
    return url;
  }
  private string CreateChart()
  {
    lock (chartValues)
    {
      ComputePoints(out var linePoints, out var fillPoints);
      XElement svg = new("svg",
        new XAttribute("height", chartHeight),
        new XAttribute("width", chartWidth),
        new XElement("defs",
          new XElement("linearGradient",
            new XAttribute("x1", "0%"),
            new XAttribute("x2", "0%"),
            new XAttribute("y1", "0%"),
            new XAttribute("y2", "100%"),
            new XAttribute("id", "gradientId"),
            new XElement("stop",
              new XAttribute("offset", "0%"),
              new XAttribute("style", StopStyles[0])
            ),
            new XElement("stop",
              new XAttribute("offset", "95%"),
              new XAttribute("style", StopStyles[1])
            )
          )
        ),
        new XElement("polyline",
          new XAttribute("points", linePoints),
          new XAttribute("style", LineStyle)
        ),
        new XElement("polyline",
          new XAttribute("points", fillPoints),
          new XAttribute("style", FillStyle)
        ),
        new XElement("rect",
          new XAttribute("height", chartHeight),
          new XAttribute("width", chartWidth),
          new XAttribute("style", "fill:none;stroke:rgb(106, 106, 106);stroke-width:1")
        )
      );

      var chartStr = svg.ToString();
      chartStr = chartStr.Replace("<svg", "<svg xmlns=\"http://www.w3.org/2000/svg\"");
      Logger.LogDebug("Chart created");
      Logger.LogTrace("{chartStr}", chartStr);
      return chartStr;
    }
  }
  private void ComputePoints(out string linePoints, out string fillPoints)
  {
    List<string> strPoints = [];
    var normalizedValues = NormalizedChartValues;
    for (var i = 0; i != capacity; ++i)
    {
      var y = (int)(chartHeight - (normalizedValues[i] * chartHeight));
      strPoints.Add($"{xCoords[i]},{y}");
    }
    linePoints = string.Join(" ", strPoints);
    fillPoints = $"{chartWidth},{chartHeight} 0,{chartHeight} {linePoints}";
  }

  public virtual void SetChartSize(Enums.ChartSize chartSize)
  {
    (chartWidth, chartHeight) = chartSize switch
    {
      Enums.ChartSize.Medium => (268, 86),
      _ => throw new NotImplementedException("Chart size not implemented")
    };
    xCoords = ComputeXCoords();
  }
  protected int[] ComputeXCoords()
  {
    var initStep = (chartWidth - 1) / (capacity - 1);
    var nRest = (chartWidth - 1) % (capacity - 1);
    var steps = new int[capacity - 1];
    Array.Fill(steps, initStep);
    if (nRest == 1)
    {
      steps[0] += 1;
    }
    else if (nRest > 1)
    {
      var restStep = (capacity - 1) / (nRest - 1);
      for (var i = 0; i < nRest; i++)
      {
        steps[i * restStep] += 1;
      }
    }
    int acc = 0;
    var ret = steps.Select(x => acc += x).Prepend(0).ToArray();
    // For beautify
    ret[capacity - 1] += 1;
    return ret;
  }
  public void AddValue(float value)
  {
    chartValues.RemoveAt(0);
    chartValues.Add(value);
  }
}

