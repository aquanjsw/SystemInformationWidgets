namespace WidgetProvider.Helpers.Records;

internal record struct DiskActivitySample(float ReadKBps = 0, float WriteKBps = 0);