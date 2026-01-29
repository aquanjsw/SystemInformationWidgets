using Microsoft.Extensions.Logging;
using Microsoft.Windows.Widgets.Providers;
using WidgetProvider.Helpers;
using WidgetProvider.Widgets;

namespace WidgetProvider;

using WidgetDefinitionId = string;
using WidgetId = string;

internal partial class WidgetProvider : IWidgetProvider, IDisposable
{
  public void Dispose()
  {
    foreach (var widgetId in RunningWidgets.Keys)
    {
      DeleteWidget(widgetId, string.Empty);
    }
  }

  private readonly ILogger _logger;
  private readonly Dictionary<WidgetDefinitionId, IWidgetFactory> _widgetCreators = [];
  private static readonly Dictionary<WidgetId, IWidget> RunningWidgets = [];
  private static readonly ManualResetEvent EmptyWidgetEvent = new(false);

  public WidgetProvider()
  {
    _logger = Utils.LoggerFactory.CreateLogger<WidgetProvider>();

    _widgetCreators.Add("NetworkActivity", new WidgetFactory<NetworkActivityWidget>());
    _widgetCreators.Add("DiskRwActivity", new WidgetFactory<DiskRwActivityWidget>());
    _widgetCreators.Add("DiskTimeActivity", new WidgetFactory<DiskTimeActivityWidget>());

    foreach (var widgetInfo in WidgetManager.GetDefault().GetWidgetInfos())
    {
      if (!RunningWidgets.ContainsKey(widgetInfo.WidgetContext.Id))
      {
        CreateWidget(widgetInfo.WidgetContext);
      }
    }
  }

  public void CreateWidget(WidgetContext widgetContext)
  {
    if (!_widgetCreators.TryGetValue(widgetContext.DefinitionId, out var widgetCreator))
    {
      _logger.LogError("Unknown widget: {widgetDefinitionId}", widgetContext.DefinitionId);
      return;
    }

    if (RunningWidgets.ContainsKey(widgetContext.Id))
    {
      _logger.LogWarning("Widget already running: {widgetDefinitionId} - {widgetId}", widgetContext.DefinitionId,
        widgetContext.Id);
      return;
    }

    var widget = widgetCreator.CreateWidget(widgetContext);
    _logger.LogInformation("Widget created: {widgetDefinitionId} - {widgetId}", widgetContext.DefinitionId,
      widgetContext.Id);
    RunningWidgets.Add(widgetContext.Id, widget);
  }

  public void Activate(WidgetContext widgetContext)
  {
    RunningWidgets[widgetContext.Id].Activate();
    _logger.LogInformation("Widget activated: {definitionId} - {id}", widgetContext.DefinitionId, widgetContext.Id);
  }

  public void Deactivate(string widgetId)
  {
    RunningWidgets[widgetId].Deactivate();
    _logger.LogInformation("Widget deactivated: {widgetId}", widgetId);
  }

  public void DeleteWidget(string widgetId, string _)
  {
    RunningWidgets[widgetId].Dispose();
    RunningWidgets.Remove(widgetId);
    _logger.LogInformation("Widget deleted: {widgetId}", widgetId);
    if (RunningWidgets.Count == 0)
    {
      EmptyWidgetEvent.Set();
      _logger.LogInformation("No more running widgets. Signaling empty widget event.");
    }
  }

  public void OnActionInvoked(WidgetActionInvokedArgs args)
  {
    RunningWidgets[args.WidgetContext.Id].OnActionInvoked(args);
    _logger.LogInformation("Action invoked on widget: {widgetId} - {widgetDefinitionId}, action: {verb}",
      args.WidgetContext.Id, args.WidgetContext.DefinitionId, args.Verb);
  }

  public void OnWidgetContextChanged(WidgetContextChangedArgs args)
  {
    RunningWidgets[args.WidgetContext.Id].OnWidgetContextChanged(args);
    _logger.LogInformation("Widget context changed: {widgetId} - {widgetDefinitionId}, size: {size}",
      args.WidgetContext.Id, args.WidgetContext.DefinitionId, args.WidgetContext.Size);
  }

  public static ManualResetEvent GetEmptyWidgetEvent() => EmptyWidgetEvent;
}