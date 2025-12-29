using Microsoft.Extensions.Logging;
using Microsoft.Windows.ApplicationModel.Resources;

namespace WidgetProvider.Helpers;

internal class Resources
{
  public static ILogger? logger;
  private static ResourceLoader? resourceLoader;
  private static readonly string[] resourceIdentifiers = [
    "NetworkUsageMonitorWidgetTemplate.Send",
    "NetworkUsageMonitorWidgetTemplate.Recv"
  ];
  private static ILogger GetLogger()
  {
    logger ??= Providers.GetLoggerFactory().CreateLogger<Resources>();
    return logger;
  }
  private static ResourceLoader GetResourceLoader()
  {
    GetLogger().LogDebug("ResourceFilePath: {ResourceFilePath}", ResourceLoader.GetDefaultResourceFilePath());
    resourceLoader ??= new(ResourceLoader.GetDefaultResourceFilePath(), "Resources");
    return resourceLoader;
  }
  public static string ReplaceIdentifiers(string template)
  {
    GetLogger().LogDebug("Replacing resource identifiers in template");
    foreach (var identifier in resourceIdentifiers)
    {
      template = template.Replace($"%{identifier}%", GetResourceLoader().GetString(identifier));
    }
    GetLogger().LogDebug("Resource identifiers replaced");
    return template;
  }
}

