using Microsoft.Extensions.Logging;
using WidgetProvider.Helpers;

namespace WidgetProvider.Widgets;

internal partial class DiskActivityWidget : WidgetBase
{
  protected override DiskActivityDataManager DataManager => _dataManager;
  protected override ILogger Logger => Utils.LoggerFactory.CreateLogger<DiskActivityWidget>();
  protected override string RelativeTemplatePath => @"Widgets\Templates\DiskActivityWidgetTemplate.json";

  private readonly DiskActivityDataManager _dataManager = new();
}