using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace WidgetProvider.Helpers.Charts;

public class DiskTimeActivityChart : ChartBase<float>
{
  protected override string CreateChart()
  {
    ComputePoints(out var linePoints, out var fillPoints);
    return new XElement(Ns + "svg",
      new XAttribute("height", ChartHeight),
      new XAttribute("width", ChartWidth),
      CreateFillPointsElement(fillPoints),
      CreateLinePointsElement(linePoints),
      ChartBorder
    ).ToString();
  }

  protected override string MainColor => "#6DAB01";
  protected override ILogger Logger => _logger;

  private void ComputePoints(out string linePoints, out string fillPoints)
  {
    float[] samplesCopy;
    lock (SamplesLock)
    {
      samplesCopy = Samples.ToArray();
    }

    var pointStrings = new string[Capacity];
    for (var i = 0; i != Capacity; ++i)
    {
      pointStrings[i] = $"{XCoords[i]},{(int)(ChartHeight - samplesCopy[i] * ChartHeight)}";
    }

    linePoints = string.Join(" ", pointStrings);
    fillPoints = $"{ChartWidth},{ChartHeight} 0,{ChartHeight} " + linePoints;
  }

  private readonly ILogger _logger = Utils.LoggerFactory.CreateLogger<DiskTimeActivityChart>();
}