namespace WidgetProvider.Widgets;

internal abstract class TimedWidgetBase : WidgetBase , IDisposable
{
  private readonly System.Timers.Timer _updateTimer = new(1500);

  protected TimedWidgetBase()
  {
    _updateTimer.Elapsed += (_, _) => UpdateWidget();
  }

  protected override void UpdateWidget()
  {
    _updateTimer.Stop();
    base.UpdateWidget();
    _updateTimer.Start();
  }
  public override void Activate()
  {
    _updateTimer.Start();
  }
  public override void Deactivate()
  {
    _updateTimer.Stop();
  }
  public virtual void Dispose()
  {
    _updateTimer.Dispose();
  }
}
