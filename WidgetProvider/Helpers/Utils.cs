using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WidgetProvider.Helpers;

internal static class Utils
{
  public static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
  {
    var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
    var configuration = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile($"appsettings_{env}.json", optional: true)
      .Build();
    builder.AddConfiguration(configuration.GetSection("Logging"));
    builder.AddConsole();
  });
}