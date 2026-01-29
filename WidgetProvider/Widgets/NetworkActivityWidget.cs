using Microsoft.Extensions.Logging;
using WidgetProvider.Helpers;
using NetworkActivityDataManager = WidgetProvider.Helpers.DataManagers.NetworkActivityDataManager;

namespace WidgetProvider.Widgets;

internal sealed partial class NetworkActivityWidget : WidgetBase
{
  protected override ILogger Logger => _logger;
  protected override NetworkActivityDataManager DataManager => _dataManager;
  protected override string RelativeTemplatePath => @"Widgets\Templates\NetworkActivityWidgetTemplate.json";

  private readonly NetworkActivityDataManager _dataManager = new();
  private readonly ILogger _logger = Utils.LoggerFactory.CreateLogger<NetworkActivityWidget>();
}