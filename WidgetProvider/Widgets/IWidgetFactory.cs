using Microsoft.Windows.Widgets.Providers;

namespace WidgetProvider.Widgets;
internal interface IWidgetFactory
{
  IWidget CreateWidget(WidgetContext widgetContext);
}
