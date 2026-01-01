using Microsoft.Extensions.Logging;
using Microsoft.Windows.Widgets.Providers;
using System.Text.Json;

namespace WidgetProvider.Widgets;

internal partial class NetworkUsageWidget : AbstractWidget, IDisposable
{
  private readonly ILogger logger = Helpers.Providers.GetLoggerFactory().CreateLogger<NetworkUsageWidget>();
  private readonly Helpers.NetworkUsageDataManager dataManager = new();
  protected override string RelativeTemplatePath => @"Widgets\Templates\NetworkUsageWidgetTemplate.json";
  private readonly System.Timers.Timer updateTimer = new(1500);
  private bool isRedetectActionEnabled = true;
  public NetworkUsageWidget() : base()
  {
    updateTimer.Elapsed += (_, _) => UpdateWidget();
  }
  public override string GetData() => JsonSerializer.Serialize(new
  {
    sentSpeed = dataManager.GetSentSpeed(),
    recvSpeed = dataManager.GetRecvSpeed(),
    isRedetectEnabled = isRedetectActionEnabled,
    interfaceName = dataManager.Interface
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
  }
  public override void OnActionInvoked(WidgetActionInvokedArgs args)
  {
    if (args.Verb == "redetect")
    {
      updateTimer.Stop();
      isRedetectActionEnabled = false;

      UpdateWidget();
      dataManager.UpdateCounters();

      isRedetectActionEnabled = true;
      updateTimer.Start();

      logger.LogInformation("Redetect action invoked.");
    }
  }
  public void Dispose()
  {
    dataManager.Dispose();
    updateTimer.Dispose();
  }
}

