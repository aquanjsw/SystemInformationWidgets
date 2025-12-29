using Microsoft.Windows.Widgets.Providers;

namespace WidgetProvider.Widgets
{
  internal interface IWidgetInterface
  {
    string Id { get; set; }
    string DefinitionId { get; set; }
    string GetTemplate();
    string GetData();
    void CreateWidget(WidgetContext widgetContext);
    void Activate();
    void Deactivate();
    void DeleteWidget();
    void OnActionInvoked(WidgetActionInvokedArgs args);
  }
}
