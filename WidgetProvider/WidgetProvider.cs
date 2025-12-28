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
    logger.LogInformation("Initializing WidgetProvider");

    widgetCreators.Add("Network", new WidgetInterfaceFactory<NetworkWidget>());

    /// Recover widgets
    logger.LogInformation("Recovering widgets");
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
    logger.LogInformation("Creating widget: {widgetDefinitionId} - {widgetId}", widgetContext.DefinitionId, widgetContext.Id);
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
    runningWidgets.Add(widgetContext.Id, widget);
    /// TODO: Check if there is a need to manually active the widget here
    ///       Cuz' the <c>Active</c> method should be called by the system
    ///       after the widget is created.
  }
  public void Activate(WidgetContext widgetContext)
  {
    logger.LogDebug("Activating widget: {widgetDefinitionId} - {widgetId}", widgetContext.DefinitionId, widgetContext.Id);
    runningWidgets[widgetContext.Id].Activate();
  }
  public void Deactivate(string widgetId)
  {
    logger.LogDebug("Deactivating widget: {widgetId}", widgetId);
    runningWidgets[widgetId].Deactivate();
  }
  public void DeleteWidget(string widgetId, string customState)
  {
    logger.LogInformation("Deleting widget: {widgetId}", widgetId);
    runningWidgets[widgetId].DeleteWidget();
    if (runningWidgets.Count == 0)
    {
      emptyWidgetEvent.Set();
    }
  }
  public void OnActionInvoked(WidgetActionInvokedArgs args)
  {
    // No action handling for now
  }
  public void OnWidgetContextChanged(WidgetContextChangedArgs args)
  {
    // No context change handling for now
  }
  public static ManualResetEvent GetEmptyWidgetEvent() => emptyWidgetEvent;
}

