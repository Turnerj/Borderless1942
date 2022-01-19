using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Borderless1942.Win32Extensions;

public readonly record struct Window(int ProcessId, nint Handle)
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
		PInvoke.SetWindowPos(Win32Handle, PInvoke.HWND_TOPMOST, x, y, 0, 0, flags);
		PInvoke.SendMessage(Win32Handle, PInvoke.WM_EXITSIZEMOVE, default, default);
	}

	public Rectangle GetBounds()
	{
		var windowInfo = new WINDOWINFO();
		PInvoke.GetWindowInfo(Win32Handle, ref windowInfo);
		return Rectangle.From(windowInfo.rcWindow);
	}

	public SafeHandle OnResize(Action<Window> handler)
	{
		var self = this;
		return Events.AddEventHandler(PInvoke.EVENT_SYSTEM_MOVESIZEEND, new EventTarget(Handle, ProcessId), eventArgs => handler(self));
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
