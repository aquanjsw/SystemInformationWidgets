using Microsoft.Extensions.Logging;

namespace WidgetProvider.Helpers.Charts;

public abstract class AbsoluteValuedChartBase<T> : ChartBase<T>
{
  public string CurrentUpperLimitString => UpperLimits.Keys.ElementAt(_currentUpperLimitIndex);

  protected abstract float GetMaxValue(T[] samples);
  protected abstract T[] NormalizeSamples(T[] samples);

  protected T[] GetNormalizedSamples()
  {
    T[] samplesCopy;
    lock (SamplesLock)
    {
      samplesCopy = Samples.ToArray();
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

  protected int CurrentUpperLimitValue => UpperLimits.Values.ElementAt(_currentUpperLimitIndex);
  protected abstract Dictionary<string, int> UpperLimits { get; }

  private int _currentUpperLimitIndex;
}