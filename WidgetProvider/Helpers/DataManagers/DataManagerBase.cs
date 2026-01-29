namespace WidgetProvider.Helpers.DataManagers;

public abstract class DataManagerBase<T> : IDataManager where T : new()
{
  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  public abstract Dictionary<string, string> GetData();
  public bool IsChartEnabled { get; set; } = false;

  protected abstract void DisposeManagedResources();
  protected readonly T Chart = new();

  private void Dispose(bool disposing)
  {
    if (_disposed) return;
    if (disposing)
    {
      DisposeManagedResources();
    }

    _disposed = true;
  }

  private bool _disposed;
}