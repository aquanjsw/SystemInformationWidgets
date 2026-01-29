using Microsoft.Extensions.Logging;
using WidgetProvider.Helpers;
using WidgetProvider.Helpers.DataManagers;

namespace WidgetProvider.Widgets;

public partial class DiskTimeActivityWidget : WidgetBase
{
  protected override string RelativeTemplatePath => @"Widgets\Templates\DiskTimeActivityWidgetTemplate.json";

  protected override IDataManager DataManager => _dataManager;
  protected override ILogger Logger => _logger;
  private readonly DiskTimeActivityDataManager _dataManager = new();
  private readonly ILogger _logger = Utils.LoggerFactory.CreateLogger<DiskTimeActivityWidget>();
}