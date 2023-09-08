using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

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
		return new((nint)process.MainWindowHandle);
	}
}

public readonly record struct Window(nint Handle)
{
	private HWND Win32Handle => new(Handle);

	public Monitor GetCurrentMonitor()
	{
		nint handle = PInvoke.MonitorFromWindow(Win32Handle, 0);
		return new(handle);
	}

	public void SetPosition(int x, int y)
	{
		var flags = SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE |
			SET_WINDOW_POS_FLAGS.SWP_NOOWNERZORDER | SET_WINDOW_POS_FLAGS.SWP_NOSENDCHANGING | SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED;
		PInvoke.SetWindowPos(Win32Handle, HWND.HWND_TOPMOST, x, y, 0, 0, flags);
		PInvoke.SendMessage(Win32Handle, PInvoke.WM_EXITSIZEMOVE, default, default);
	}

	public Rectangle GetBounds()
	{
		var windowInfo = new WINDOWINFO();
		PInvoke.GetWindowInfo(Win32Handle, ref windowInfo);
		return Rectangle.From(windowInfo.rcWindow);
	}

	public void RemoveBorders()
	{
		var style = PInvoke.GetWindowLong(Win32Handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
		style &= ~(int)(WINDOW_STYLE.WS_THICKFRAME | WINDOW_STYLE.WS_DLGFRAME | WINDOW_STYLE.WS_BORDER);
		_ = PInvoke.SetWindowLong(Win32Handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);

		style = PInvoke.GetWindowLong(Win32Handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
		style &= ~(int)(WINDOW_EX_STYLE.WS_EX_DLGMODALFRAME | WINDOW_EX_STYLE.WS_EX_WINDOWEDGE | WINDOW_EX_STYLE.WS_EX_CLIENTEDGE | WINDOW_EX_STYLE.WS_EX_STATICEDGE);
		_ = PInvoke.SetWindowLong(Win32Handle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, style);
		PInvoke.SendMessage(Win32Handle, PInvoke.WM_EXITSIZEMOVE, default, default);
	}
}

public readonly record struct Monitor(nint Handle)
{
	private HMONITOR Win32Handle => new(Handle);

	public unsafe Rectangle GetBounds()
	{
		var monitorInfo = new MONITORINFO
		{
			cbSize = (uint)sizeof(MONITORINFO)
		};
		PInvoke.GetMonitorInfo(Win32Handle, ref monitorInfo);
		return Rectangle.From(monitorInfo.rcMonitor);
	}
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Rectangle(int Left, int Top, int Right, int Bottom)
{
	public int Width => Right - Left;
	public int Height => Bottom - Top;

	internal static Rectangle From(RECT rectangle) => Unsafe.As<RECT, Rectangle>(ref rectangle);
}