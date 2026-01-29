namespace WidgetProvider.Helpers.Records;

internal record struct DiskRwActivitySample(float ReadKBps = 0, float WriteKBps = 0);