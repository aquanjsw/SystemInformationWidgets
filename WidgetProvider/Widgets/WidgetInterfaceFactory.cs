using Microsoft.Windows.Widgets.Providers;

namespace WidgetProvider.Widgets;
internal class WidgetInterfaceFactory<T>: IWidgetInterfaceFactory
  where T : IWidgetInterface, new()
{
  public IWidgetInterface CreateWidget(WidgetContext widgetContext)
  {
    var widget = new T();
    widget.CreateWidget(widgetContext);
    return widget;
  }
}
