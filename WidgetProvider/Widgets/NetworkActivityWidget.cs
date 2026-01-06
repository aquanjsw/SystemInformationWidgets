using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WidgetProvider.Widgets;

internal partial class NetworkActivityWidget : TimedWidgetBase
{
  protected override ILogger Logger => Helpers.Providers.GetLoggerFactory().CreateLogger<NetworkActivityWidget>();
  private readonly Helpers.NetworkActivityDataManager dataManager = new();
  protected override string RelativeTemplatePath => @"Widgets\Templates\NetworkActivityWidgetTemplate.json";
  public override string GetData() => JsonSerializer.Serialize(new
  {
    sentSpeed = dataManager.GetSentSpeed(),
    recvSpeed = dataManager.GetRecvSpeed(),
  });
  public override void Dispose()
  {
    dataManager.Dispose();
    base.Dispose();
  }
}

