using Microsoft.Extensions.Logging;
using Microsoft.Windows.Widgets.Providers;
using System.Text.Json;

namespace WidgetProvider.Widgets;

internal partial class NetworkActivityWidget : TimedWidgetBase
{
  protected override ILogger Logger => Helpers.Providers.GetLoggerFactory().CreateLogger<NetworkActivityWidget>();
  private readonly Helpers.NetworkActivityDataManager dataManager = new();
  protected override string RelativeTemplatePath => @"Widgets\Templates\NetworkActivityWidgetTemplate.json";
  public override string GetData() => JsonSerializer.Serialize(dataManager.GetData());

  public override void Dispose()
  {
    dataManager.Dispose();
    base.Dispose();
  }

  public override void OnWidgetContextChanged(WidgetContextChangedArgs args)
  {
    if (args.WidgetContext.Size == Microsoft.Windows.Widgets.WidgetSize.Medium)
    {
      dataManager.IsChartEnabled = true;
      Logger.LogInformation("Chart enabled");
    }
    else
    {
      dataManager.IsChartEnabled = false;
      Logger.LogInformation("Chart disabled");
    }

    base.OnWidgetContextChanged(args);
  }

  public override void Deactivate()
  {
    if (dataManager.IsChartEnabled)
    {
      return;
    }

    base.Deactivate();
  }

  public override void Activate()
  {
    if (dataManager.IsChartEnabled)
    {
      return;
    }

    base.Activate();
  }
}