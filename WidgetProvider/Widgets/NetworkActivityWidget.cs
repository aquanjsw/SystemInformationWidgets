using Microsoft.Extensions.Logging;
using Microsoft.Windows.Widgets.Providers;
using System.Text.Json;

namespace WidgetProvider.Widgets;

internal partial class NetworkActivityWidget : AbstractWidget, IDisposable
{
  protected override ILogger Logger => Helpers.Providers.GetLoggerFactory().CreateLogger<NetworkActivityWidget>();
  private readonly Helpers.NetworkActivityDataManager dataManager = new();
  protected override string RelativeTemplatePath => @"Widgets\Templates\NetworkActivityWidgetTemplate.json";
  private readonly System.Timers.Timer updateTimer = new(1500);
  private bool isRedetectActionEnabled = true;
  public NetworkActivityWidget() : base()
  {
    updateTimer.Elapsed += (_, _) => UpdateWidget();
  }
  public override string GetData() => JsonSerializer.Serialize(new
  {
    sentSpeed = dataManager.GetSentSpeed(),
    recvSpeed = dataManager.GetRecvSpeed(),
    isRedetectActionEnabled,
    interfaceName = dataManager.Interface
  });
  public override void Activate()
  {
    updateTimer.Start();
    Logger.LogTrace("timer for dataManager started.");
  }
  public override void Deactivate()
  {
    updateTimer.Stop();
    Logger.LogTrace("timer for dataManager stopped.");
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

      Logger.LogInformation("Redetect action invoked.");
    }
  }
  public void Dispose()
  {
    dataManager.Dispose();
    updateTimer.Dispose();
  }
}

