using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WidgetProvider.Widgets;

internal partial class NetworkWidget : AbstractWidget, IDisposable
{
  private readonly ILogger logger;
  private readonly Helpers.NetworkDataManager dataManager = new();
  protected override string RelativeTemplatePath => @"Widgets\Templates\NetworkWidgetTemplate.json";
  private readonly System.Timers.Timer updateTimer = new(1500);
  public NetworkWidget() : base()
  {
    logger = Helpers.Providers.GetLoggerFactory().CreateLogger<NetworkWidget>();
    updateTimer.Elapsed += (_, _) => UpdateWidget();
  }
  public override string GetData() => JsonSerializer.Serialize(new
  {
    sentSpeed = dataManager.GetSentSpeed(),
    recvSpeed = dataManager.GetRecvSpeed()
  });
  public override void Activate()
  {
    updateTimer.Start();
    logger.LogTrace("timer for dataManager started.");
  }
  public override void Deactivate()
  {
    updateTimer.Stop();
    logger.LogTrace("timer for dataManager stopped.");
  }
  public override void DeleteWidget()
  {
    base.DeleteWidget();
    updateTimer.Stop();
  }
  public void Dispose()
  {
    dataManager.Dispose();
    updateTimer.Dispose();
  }
}

