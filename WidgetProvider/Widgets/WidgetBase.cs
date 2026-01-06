using Microsoft.Extensions.Logging;
using Microsoft.Windows.Widgets.Providers;

namespace WidgetProvider.Widgets;

internal abstract class WidgetBase : IWidgetInterface
{
  protected abstract ILogger Logger { get; }
  public string Id { get; set; } = "";
  public string DefinitionId { get; set; } = "";
  protected readonly string template;
  protected abstract string RelativeTemplatePath { get; }
  public WidgetBase()
  {
    var templatePath = Path.Combine(Windows.ApplicationModel.Package.Current.EffectivePath, RelativeTemplatePath);
    template = File.ReadAllText(templatePath);
    template = Helpers.Resources.Localize(template);
  }
  public virtual string GetTemplate() => template;
  public abstract string GetData();
  public virtual void CreateWidget(WidgetContext widgetContext)
  {
    Logger.LogInformation("Creating widget: {definitionId} - {id}", widgetContext.DefinitionId, widgetContext.Id);
    Id = widgetContext.Id;
    DefinitionId = widgetContext.DefinitionId;
  }
  protected void UpdateWidget()
  {
    WidgetUpdateRequestOptions options = new(Id)
    {
      Template = GetTemplate(),
      Data = GetData()
    };
    WidgetManager.GetDefault().UpdateWidget(options);
  }
  public abstract void Activate();
  public abstract void Deactivate();
  public virtual void DeleteWidget()
  {
    Deactivate();
  }
  public virtual void OnActionInvoked(WidgetActionInvokedArgs args) { }
}
