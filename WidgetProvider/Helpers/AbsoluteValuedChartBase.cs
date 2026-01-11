using Microsoft.Extensions.Logging;

namespace WidgetProvider.Helpers;

internal abstract class AbsoluteValuedChartBase : ChartBase
{
  protected abstract Dictionary<string, int> UpperLimits { get; }
  protected abstract float DecayThreshold { get; }
  protected int currentUpperLimitIndex = 0;
  public string CurrentUpperLimitString => UpperLimits.Keys.ElementAt(currentUpperLimitIndex);
  protected int CurrentUpperLimitValue => UpperLimits.Values.ElementAt(currentUpperLimitIndex);
  public AbsoluteValuedChartBase() : base() { }
  public abstract string Value2String(float value);
  protected override List<float> NormalizedChartValues
  {
    get
    {
      var max = chartValues.Max();
      if (max > CurrentUpperLimitValue)
      {
        if (currentUpperLimitIndex < UpperLimits.Count - 1)
        {
          currentUpperLimitIndex++;
          Logger.LogInformation("Increased upper limit to {UpperLimit}", CurrentUpperLimitString);
        }
      }
      else if (max < CurrentUpperLimitValue * DecayThreshold)
      {
        if (currentUpperLimitIndex != 0)
        {
          currentUpperLimitIndex--;
          Logger.LogInformation("Decreased upper limit to {UpperLimit}", CurrentUpperLimitString);
        }
      }
      return [.. chartValues.Select(v => v / CurrentUpperLimitValue)];
    }
  }
}
