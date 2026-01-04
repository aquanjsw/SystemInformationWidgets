using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WidgetProvider.Helpers;

internal static partial class Resources
{
  private static readonly ResourceManager resourceManager = new("WidgetProvider.Resources.Strings", Assembly.GetExecutingAssembly());
  private static readonly Lazy<ILogger> lazyLogger = new(() => Providers.GetLoggerFactory().CreateLogger("Resources"));

  [GeneratedRegex(@"\{\{(?<key>[a-zA-Z\d_]+)\}\}")]
  private static partial Regex LocalizationRegex();
  public static string Localize(string src)
  {
    var cul = CultureInfo.CurrentUICulture;
    lazyLogger.Value.LogInformation("Localizing: {Culture}", cul);
    var result = LocalizationRegex().Replace(src, match => { 
      var key = match.Groups["key"].Value;
      var value = resourceManager.GetString(key)!;
      lazyLogger.Value.LogDebug("Localize: {Key} => {Value}", key, value);
      return value;
    });
    return result;
  }
}

