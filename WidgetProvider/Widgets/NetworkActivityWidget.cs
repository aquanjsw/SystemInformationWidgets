using Microsoft.Extensions.Logging;
using WidgetProvider.Helpers;

namespace WidgetProvider.Widgets;

internal sealed partial class NetworkActivityWidget : WidgetBase
{
  protected override ILogger Logger => Utils.LoggerFactory.CreateLogger<NetworkActivityWidget>();
  protected override NetworkActivityDataManager DataManager => _dataManager;
  protected override string RelativeTemplatePath => @"Widgets\Templates\NetworkActivityWidgetTemplate.json";

  private readonly NetworkActivityDataManager _dataManager = new();
}