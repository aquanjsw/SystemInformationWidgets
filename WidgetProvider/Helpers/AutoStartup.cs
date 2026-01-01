using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Reflection;

namespace WidgetProvider.Helpers;

internal class AutoStartup
{
  private const string AppName = "SystemWidgetProvider";
  private static readonly Lazy<ILogger> lazyLogger = new(() => Providers.GetLoggerFactory().CreateLogger<AutoStartup>());
  public static void Setup() {
    var exePath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
    var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
    if (regKey == null) {
      lazyLogger.Value.LogWarning("Failed to open registry key for setting auto startup.");
      return;
    }
    regKey.SetValue(AppName, $"\"{exePath}\"");
    lazyLogger.Value.LogInformation("Set auto startup for {AppName} with path: {ExePath}", AppName, exePath);
  }
}
