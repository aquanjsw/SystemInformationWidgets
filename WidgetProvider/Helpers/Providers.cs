using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WidgetProvider.Helpers;

internal class Providers
{
  private static ILoggerFactory? loggerFactory;
  private static IConfigurationRoot? configuration;
  public static ILoggerFactory GetLoggerFactory()
  {
    loggerFactory ??= LoggerFactory.Create(builder =>
    {
      builder.AddConfiguration(GetConfiguration().GetSection("Logging"));
      builder.AddConsole();
    });
    return loggerFactory;
  }
  public static IConfigurationRoot GetConfiguration()
  {
    var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
    configuration ??= new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile($"appsettings_{env}.json", optional: true)
      .Build();
    return configuration;
  }
}

