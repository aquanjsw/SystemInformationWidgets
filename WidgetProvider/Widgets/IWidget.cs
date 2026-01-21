using Microsoft.Windows.Widgets.Providers;

namespace WidgetProvider.Widgets;

internal interface IWidget : IDisposable
{
  string Id { get; set; }
  string DefinitionId { get; set; }
  string GetTemplate();
  string GetData();
  void CreateWidget(WidgetContext widgetContext);
  void Activate();
  void Deactivate();
  void OnActionInvoked(WidgetActionInvokedArgs args);
  void OnWidgetContextChanged(WidgetContextChangedArgs args);
}