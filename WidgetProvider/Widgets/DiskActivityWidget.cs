using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WidgetProvider.Widgets;

internal partial class DiskActivityWidget : TimedWidgetBase
{
  protected override ILogger Logger => Helpers.Providers.GetLoggerFactory().CreateLogger<DiskActivityWidget>();
  protected override string RelativeTemplatePath => @"Widgets\Templates\DiskActivityWidgetTemplate.json";
  private readonly Helpers.DiskActivityDataManager dataManager = new();
  public override string GetData() => JsonSerializer.Serialize(new
  {
    readSpeed = dataManager.GetReadSpeed(),
    writeSpeed = dataManager.GetWriteSpeed()
  });
  public override void Dispose()
  {
    dataManager.Dispose();
    base.Dispose();
  }
}
