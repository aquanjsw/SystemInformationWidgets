using Microsoft.Extensions.Logging;
using WidgetProvider.Helpers;
using DiskRwActivityDataManager = WidgetProvider.Helpers.DataManagers.DiskRwActivityDataManager;

namespace WidgetProvider.Widgets;

internal partial class DiskRwActivityWidget : WidgetBase
{
  protected override DiskRwActivityDataManager DataManager => _dataManager;
  protected override ILogger Logger => _logger;
  protected override string RelativeTemplatePath => @"Widgets\Templates\DiskRwActivityWidgetTemplate.json";

  private readonly DiskRwActivityDataManager _dataManager = new();
  private readonly ILogger _logger = Utils.LoggerFactory.CreateLogger<DiskRwActivityWidget>();
}