using Microsoft.Windows.Widgets.Providers;

namespace WidgetProvider.Widgets;

internal abstract class AbstractWidget : IWidgetInterface
{
  public string Id { get; set; } = "";
  public string DefinitionId { get; set; } = "";
  protected readonly string template;
  protected abstract string RelativeTemplatePath { get; }
  public AbstractWidget()
  {
    var templatePath = Path.Combine(Windows.ApplicationModel.Package.Current.EffectivePath, RelativeTemplatePath);
    template = File.ReadAllText(templatePath);
    // Do not need for localization support now
    //template = Helpers.Resources.ReplaceIdentifiers(template);
  }
  public virtual string GetTemplate() => template;
  public abstract string GetData();
  public virtual void CreateWidget(WidgetContext widgetContext)
  {
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
}
