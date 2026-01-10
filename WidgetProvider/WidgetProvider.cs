using Microsoft.Extensions.Logging;
using Microsoft.Windows.Widgets.Providers;

namespace WidgetProvider;

using Widgets;
using WidgetDefinitionId = string;
using WidgetId = string;
internal partial class WidgetProvider : IWidgetProvider
{
  private readonly ILogger logger;
  private readonly Dictionary<WidgetDefinitionId, IWidgetInterfaceFactory> widgetCreators = [];
  private readonly static Dictionary<WidgetId, IWidgetInterface> runningWidgets = [];
  private readonly static ManualResetEvent emptyWidgetEvent = new(false);
  public WidgetProvider()
  {
    logger = Helpers.Providers.GetLoggerFactory().CreateLogger<WidgetProvider>();

    widgetCreators.Add("NetworkActivity", new WidgetInterfaceFactory<NetworkActivityWidget>());
    widgetCreators.Add("DiskActivity", new WidgetInterfaceFactory<DiskActivityWidget>());

    /// Recover widgets
    foreach (var widgetInfo in WidgetManager.GetDefault().GetWidgetInfos())
    {
      if (!runningWidgets.ContainsKey(widgetInfo.WidgetContext.Id))
      {
        CreateWidget(widgetInfo.WidgetContext);
      }
    }
  }
  public void CreateWidget(WidgetContext widgetContext)
  {
    if (!widgetCreators.TryGetValue(widgetContext.DefinitionId, out var widgetCreator))
    {
      logger.LogError("Unknown widget: {widgetDefinitionId}", widgetContext.DefinitionId);
      return;
    }
    if (runningWidgets.ContainsKey(widgetContext.Id))
    {
      logger.LogWarning("Widget already running: {widgetDefinitionId} - {widgetId}", widgetContext.DefinitionId, widgetContext.Id);
      return;
    }
    var widget = widgetCreator.CreateWidget(widgetContext);
    logger.LogInformation("Widget created: {widgetDefinitionId} - {widgetId}", widgetContext.DefinitionId, widgetContext.Id);
    runningWidgets.Add(widgetContext.Id, widget);
  }
  public void Activate(WidgetContext widgetContext)
  {
    runningWidgets[widgetContext.Id].Activate();
    logger.LogInformation("Widget activated: {definitionId} - {id}", widgetContext.DefinitionId, widgetContext.Id);
  }
  public void Deactivate(string widgetId)
  {
    runningWidgets[widgetId].Deactivate();
    logger.LogInformation("Widget deactivated: {widgetId}", widgetId);
  }
  public void DeleteWidget(string widgetId, string customState)
  {
    runningWidgets[widgetId].DeleteWidget();
    logger.LogInformation("Widget deleted: {widgetId}", widgetId);
    if (runningWidgets.Count == 0)
    {
      emptyWidgetEvent.Set();
      logger.LogInformation("No more running widgets. Signaling empty widget event.");
    }
  }
  public void OnActionInvoked(WidgetActionInvokedArgs args)
  {
    runningWidgets[args.WidgetContext.Id].OnActionInvoked(args);
    logger.LogInformation("Action invoked on widget: {widgetId} - {widgetDefinitionId}, action: {verb}", args.WidgetContext.Id, args.WidgetContext.DefinitionId, args.Verb);
  }
  public void OnWidgetContextChanged(WidgetContextChangedArgs args)
  {
    runningWidgets[args.WidgetContext.Id].OnWidgetContextChanged(args);
    logger.LogInformation("Widget context changed: {widgetId} - {widgetDefinitionId}, size: {size}", args.WidgetContext.Id, args.WidgetContext.DefinitionId, args.WidgetContext.Size);
  }
  public static ManualResetEvent GetEmptyWidgetEvent() => emptyWidgetEvent;
}

