using COM;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using WidgetProvider.Helpers;

AutoStartup.Setup();

var logger = Providers.GetLoggerFactory().CreateLogger<Program>();

[DllImport("kernel32.dll")]
static extern IntPtr GetConsoleWindow();

[DllImport("ole32.dll")]
static extern int CoRegisterClassObject(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
            [MarshalAs(UnmanagedType.IUnknown)] object pUnk,
            uint dwClsContext,
            uint flags,
            out uint lpdwRegister);

[DllImport("ole32.dll")] static extern int CoRevokeClassObject(uint dwRegister);

uint cookie;

Guid CLSID_Factory = Guid.Parse("88C44EA9-032B-4443-A7ED-21182BAD7079");
var errno = CoRegisterClassObject(CLSID_Factory, new WidgetProviderFactory<WidgetProvider.WidgetProvider>(), 0x4, 0x1, out cookie);
if (errno != 0)
{
  throw new Exception("Failed to register class object. Error code: " + errno);
}

if (GetConsoleWindow() != IntPtr.Zero)
{
  logger.LogInformation("Registered successfully. Press ENTER to exit.");
  Console.ReadLine();
}
else
{
  // Wait until the manager has disposed of the last widget provider.
  using (var emptyWidgetEvent = WidgetProvider.WidgetProvider.GetEmptyWidgetEvent())
  {
    emptyWidgetEvent.WaitOne();
  }

  errno = CoRevokeClassObject(cookie);
  if (errno != 0)
  {
    throw new Exception("Failed to revoke class object. Error code: " + errno);
  }
}

