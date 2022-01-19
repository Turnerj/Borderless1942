using System.Diagnostics;
using Windows.Win32.Foundation;

namespace Borderless1942.Win32Extensions;

public static class Extensions
{
	public static async ValueTask<Window> WaitForMainWindowAsync(this Process process)
	{
		while (process.MainWindowHandle == IntPtr.Zero)
		{
			await Task.Delay(100);
		}
		return process.GetMainWindow();
	}

	public unsafe static Window GetMainWindow(this Process process)
	{
		return new(process.Id, (nint)process.MainWindowHandle);
	}

	internal static bool IsTrue(this BOOL value) => value.Value != 0;
	internal static bool IsFalse(this BOOL value) => value.Value == 0;
}
