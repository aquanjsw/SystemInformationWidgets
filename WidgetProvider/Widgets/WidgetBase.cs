using System.Text.Json;
using Windows.ApplicationModel;
using Microsoft.Extensions.Logging;
using Microsoft.Windows.Widgets;
using Microsoft.Windows.Widgets.Providers;
using WidgetProvider.Helpers;
using Timer = System.Timers.Timer;

namespace WidgetProvider.Widgets;

internal abstract class WidgetBase : IWidget
{
  public virtual void Activate()
  {
    if (!DataManager.IsChartEnabled)
    {
      _updateTimer.Start();
    }
  }

  public virtual void Deactivate()
  {
    if (!DataManager.IsChartEnabled)
    {
      _updateTimer.Stop();
    }
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  public virtual void OnActionInvoked(WidgetActionInvokedArgs args)
  {
  }

  public virtual void OnWidgetContextChanged(WidgetContextChangedArgs args)
  {
    if (args.WidgetContext.Size == WidgetSize.Medium)
    {
      DataManager.IsChartEnabled = true;
      Logger.LogInformation("Chart enabled");
    }
    else
    {
      DataManager.IsChartEnabled = false;
      Logger.LogInformation("Chart disabled");
    }
  }

  public virtual string GetTemplate() => _template;
  public string GetData() => JsonSerializer.Serialize(DataManager.GetData());

  public virtual void CreateWidget(WidgetContext widgetContext)
  {
    Logger.LogInformation("Creating widget: {definitionId} - {id}", widgetContext.DefinitionId, widgetContext.Id);
    Id = widgetContext.Id;
    DefinitionId = widgetContext.DefinitionId;
  }

  public string Id { get; set; } = "";
  public string DefinitionId { get; set; } = "";

  protected WidgetBase()
  {
    var templatePath = Path.Combine(Package.Current.EffectivePath, RelativeTemplatePath);
    _template = File.ReadAllText(templatePath);
    _template = Helpers.Resources.Localize(_template);
    Logger.LogInformation("Widget template loaded from: {path}", templatePath);
    Logger.LogDebug("Widget template content: {template}", _template);

    _updateTimer.Elapsed += (_, _) => UpdateWidget();
  }

  protected abstract string RelativeTemplatePath { get; }
  protected abstract IDataManager DataManager { get; }
  protected abstract ILogger Logger { get; }

  private void Dispose(bool disposing)
  {
    if (_disposed) return;
    if (disposing)
    {
      DataManager.Dispose();
      _updateTimer.Dispose();
    }

    _disposed = true;
  }

  private void UpdateWidget()
  {
    Logger.LogDebug("Getting widget data...");
    var data = GetData();
    Logger.LogTrace("Updating widget data: {data}", data);
    WidgetUpdateRequestOptions options = new(Id)
    {
      Template = GetTemplate(),
      Data = data
    };
    WidgetManager.GetDefault().UpdateWidget(options);
  }

  private readonly string _template;
  private readonly Timer _updateTimer = new(1000);
  private bool _disposed;
}