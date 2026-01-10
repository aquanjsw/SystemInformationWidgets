using Microsoft.Extensions.Logging;
using System.Numerics;

namespace WidgetProvider.Helpers;

internal abstract class AbsoluteValuedChartBase : ChartBase
{
  protected abstract int[] UpperLimits { get; }
  protected const float decayThreshold = 0.2f;
  protected int currentUpperLimitIndex = 0;
  public string CurrentUpperLimitString => Value2String(UpperLimits[currentUpperLimitIndex], 0);
  public AbsoluteValuedChartBase() : base()
  {
  }
  public abstract string Value2String(float value, int precision);
  protected override List<float> NormalizedChartValues
  {
    get
    {
      var upperLimit = UpperLimits[currentUpperLimitIndex];
      var max = chartValues.Max();
      if (max > upperLimit)
      {
        if (currentUpperLimitIndex < UpperLimits.Length - 1)
        {
          currentUpperLimitIndex++;
          upperLimit = UpperLimits[currentUpperLimitIndex];
          Logger.LogInformation("Increased upper limit to {UpperLimit}", Value2String(UpperLimits[currentUpperLimitIndex], 0));
        }
      }
      else if (max < upperLimit * decayThreshold)
      {
        if (currentUpperLimitIndex != 0)
        {
          currentUpperLimitIndex--;
          upperLimit = UpperLimits[currentUpperLimitIndex];
          Logger.LogInformation("Decreased upper limit to {UpperLimit}", Value2String(UpperLimits[currentUpperLimitIndex], 0));
        }
      }

      return [.. chartValues.Select(v => v / upperLimit)];
    }
  }
}
