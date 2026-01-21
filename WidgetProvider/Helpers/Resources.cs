using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace WidgetProvider.Helpers;

internal static partial class Resources
{
  private static readonly ResourceManager ResourceManager =
    new("WidgetProvider.Resources.Strings", Assembly.GetExecutingAssembly());

  private static readonly ILogger Logger = Utils.LoggerFactory.CreateLogger("Resources");

  [GeneratedRegex(@"\{\{(?<key>[a-zA-Z\d_]+)\}\}")]
  private static partial Regex LocalizationRegex();

  public static string Localize(string src)
  {
    var cul = CultureInfo.CurrentUICulture;
    Logger.LogInformation("Localizing: {Culture}", cul);
    var result = LocalizationRegex().Replace(src, match =>
    {
      var key = match.Groups["key"].Value;
      var value = ResourceManager.GetString(key)!;
      Logger.LogDebug("Localize: {Key} => {Value}", key, value);
      return value;
    });
    return result;
  }
}