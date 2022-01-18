using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace Borderless1942;

public static class Win32Extensions
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
		var handle = new HWND(process.MainWindowHandle.ToPointer());
		return new(handle);
	}
}

public unsafe readonly record struct Window(HWND Handle)
{
	public Monitor GetCurrentMonitor()
	{
		var handle = Windows.MonitorFromWindow(Handle, 0);
		return new(handle);
	}

	public void SetPosition(int x, int y)
	{
		uint flags = SWP.SWP_NOSIZE | SWP.SWP_NOZORDER | SWP.SWP_NOACTIVATE | SWP.SWP_NOOWNERZORDER | SWP.SWP_NOSENDCHANGING | SWP.SWP_FRAMECHANGED;
		Windows.SetWindowPos(Handle, HWND.HWND_TOPMOST, x, y, 0, 0, flags);
		Windows.SendMessage(Handle, WM.WM_EXITSIZEMOVE, default, default);
	}

	public Rectangle GetBounds()
	{
		var windowInfo = new WINDOWINFO();
		Windows.GetWindowInfo(Handle, &windowInfo);
		return Rectangle.From(windowInfo.rcWindow);
	}

	public void RemoveBorders()
	{
		var style = Windows.GetWindowLong(Handle, GWL.GWL_STYLE);
		style &= ~(WS.WS_THICKFRAME | WS.WS_DLGFRAME | WS.WS_BORDER);
		Windows.SetWindowLong(Handle, GWL.GWL_STYLE, style);

		style = Windows.GetWindowLong(Handle, GWL.GWL_EXSTYLE);
		style &= ~(WS.WS_EX_DLGMODALFRAME | WS.WS_EX_WINDOWEDGE | WS.WS_EX_CLIENTEDGE | WS.WS_EX_STATICEDGE);
		Windows.SetWindowLong(Handle, GWL.GWL_EXSTYLE, style);

		Windows.SendMessage(Handle, WM.WM_EXITSIZEMOVE, default, default);
	}
}

public readonly record struct Monitor(HMONITOR Handle)
{
	public unsafe Rectangle GetBounds()
	{
		var monitorInfo = new MONITORINFO
		{
			cbSize = (uint)sizeof(MONITORINFO)
		};
		Windows.GetMonitorInfoA(Handle, &monitorInfo);
		return Rectangle.From(monitorInfo.rcMonitor);
	}
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Rectangle(int Left, int Top, int Right, int Bottom)
{
	public int Width => Right - Left;
	public int Height => Bottom - Top;

	public static Rectangle From(RECT rectangle) => Unsafe.As<RECT, Rectangle>(ref rectangle);
}