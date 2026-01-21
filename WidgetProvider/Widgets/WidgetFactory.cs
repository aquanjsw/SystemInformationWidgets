using Microsoft.Windows.Widgets.Providers;

namespace WidgetProvider.Widgets;

internal class WidgetFactory<T> : IWidgetFactory
  where T : IWidget, new()
{
  public IWidget CreateWidget(WidgetContext widgetContext)
  {
    var widget = new T();
    widget.CreateWidget(widgetContext);
    return widget;
  }
}