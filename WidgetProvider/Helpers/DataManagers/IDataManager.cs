namespace WidgetProvider.Helpers.DataManagers;

public interface IDataManager : IDisposable
{
  Dictionary<string, string> GetData();
  bool IsChartEnabled { get; set; }
}