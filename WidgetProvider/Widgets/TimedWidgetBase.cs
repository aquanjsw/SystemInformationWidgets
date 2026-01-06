namespace WidgetProvider.Widgets;

internal abstract class TimedWidgetBase : WidgetBase , IDisposable
{
  protected readonly System.Timers.Timer updateTimer = new(1500);
  public TimedWidgetBase() : base()
  {
    updateTimer.Elapsed += (_, _) => UpdateWidget();
  }
  public override void Activate()
  {
    updateTimer.Start();
  }
  public override void Deactivate()
  {
    updateTimer.Stop();
  }
  public virtual void Dispose()
  {
    updateTimer.Dispose();
  }
}
