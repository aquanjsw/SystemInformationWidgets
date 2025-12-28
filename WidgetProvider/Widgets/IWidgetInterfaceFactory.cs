using Microsoft.Windows.Widgets.Providers;

namespace WidgetProvider.Widgets;
internal interface IWidgetInterfaceFactory
{
  IWidgetInterface CreateWidget(WidgetContext widgetContext);
}
