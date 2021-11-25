using SRWE;
using System.Diagnostics;
using System.Runtime.InteropServices;

// Start Process
var processStartInfo = new ProcessStartInfo
{
	FileName = @"BF1942.exe",
	//FileName = @"S:\Battlefield 1942\BF1942.exe",
	//WorkingDirectory = @"S:\Battlefield 1942\"
};
foreach (var arg in args)
{
	processStartInfo.ArgumentList.Add(arg);
}
var process = Process.Start(processStartInfo)!;
var keepAlive = true;
Console.WriteLine($"Process has started ({process.Id})");

// Wait for initial window handle
var handle = await WaitForHandleAsync(process);
RemoveWindowBorders(handle.ToInt32());

// Keep itself alive until detected otherwise
while (keepAlive)
{
MainLoop:
	UpdateWindowPosition(handle.ToInt32());
	await Task.Delay(250);

	if (process.HasExited)
	{
		var oldProcessId = process.Id;
		var retryCount = 0;
		while (retryCount < 2)
		{
			var matchingProcesses = Process.GetProcessesByName("BF1942");
			if (matchingProcesses.Length == 1)
			{
				process = matchingProcesses[0];
				Console.WriteLine($"Process has changed ({oldProcessId} -> {process.Id})");
				handle = await WaitForHandleAsync(process);
				RemoveWindowBorders(handle.ToInt32());
				goto MainLoop;
			}
			retryCount++;
			await Task.Delay(TimeSpan.FromSeconds(1));
		}
		keepAlive = false;
		Console.WriteLine($"Process has exited ({oldProcessId})");
	}
}

unsafe static void UpdateWindowPosition(int handle)
{
	var info = new WINDOWINFO();
	var success = WinAPI.GetWindowInfo(handle, ref info);
	if (success)
    {
		var windowDimensions = info.rcWindow;
		var monitorHandle = WinAPI.MonitorFromWindow(handle, 0);
		var monitorInfo = new LPMONITORINFO
        {
			cbSize = (uint)sizeof(LPMONITORINFO)
        };
		WinAPI.GetMonitorInfoA(monitorHandle, ref monitorInfo);
		var monitorDimensions = monitorInfo.rcMonitor;
		var x = monitorDimensions.Width / 2 - windowDimensions.Width / 2;
		var y = monitorDimensions.Height / 2 - windowDimensions.Height / 2;
		SetPosition(handle, x, y);
	}
}

static async Task<IntPtr> WaitForHandleAsync(Process process)
{
	var handle = process.MainWindowHandle;
	while (handle == IntPtr.Zero)
	{
		await Task.Delay(100);
		handle = process.MainWindowHandle;
	}
	return handle;
}

static void SetPosition(int handle, int x, int y)
{
	uint uFlags = WinAPI.SWP_NOSIZE | WinAPI.SWP_NOZORDER | WinAPI.SWP_NOACTIVATE | WinAPI.SWP_NOOWNERZORDER | WinAPI.SWP_NOSENDCHANGING | WinAPI.SWP_FRAMECHANGED;
	WinAPI.SetWindowPos(handle, WinAPI.HWND_TOPMOST, x, y, 0, 0, uFlags);
	WinAPI.SendMessage(handle, WinAPI.WM_EXITSIZEMOVE, 0, 0);
}

static void RemoveWindowBorders(int handle)
{
	uint nStyle = (uint)WinAPI.GetWindowLong(handle, WinAPI.GWL_STYLE);
	nStyle = nStyle & ~(WinAPI.WS_THICKFRAME | WinAPI.WS_DLGFRAME | WinAPI.WS_BORDER);
	WinAPI.SetWindowLong(handle, WinAPI.GWL_STYLE, nStyle);

	nStyle = (uint)WinAPI.GetWindowLong(handle, WinAPI.GWL_EXSTYLE);
	nStyle = nStyle & ~(WinAPI.WS_EX_DLGMODALFRAME | WinAPI.WS_EX_WINDOWEDGE | WinAPI.WS_EX_CLIENTEDGE | WinAPI.WS_EX_STATICEDGE);
	WinAPI.SetWindowLong(handle, WinAPI.GWL_EXSTYLE, nStyle);

	WinAPI.SendMessage(handle, WinAPI.WM_EXITSIZEMOVE, 0, 0);
}

namespace SRWE
{
	/// <summary>
	/// WinAPI class.
	/// </summary>
	static class WinAPI
	{
		public const int HWND_TOP = 0;
		public const int HWND_TOPMOST = -1;

		public const int GWL_STYLE = -16;
		public const int GWL_EXSTYLE = -20;

		public const uint SWP_NOSIZE = 0x01;
		public const uint SWP_NOMOVE = 0x02;
		public const uint SWP_NOZORDER = 0x04;
		public const uint SWP_NOACTIVATE = 0x10;
		public const uint SWP_NOOWNERZORDER = 0x200;
		public const uint SWP_NOSENDCHANGING = 0x400;
		public const uint SWP_FRAMECHANGED = 0x20;

		public const uint WS_THICKFRAME = 0x40000;
		public const uint WS_DLGFRAME = 0x400000;
		public const uint WS_BORDER = 0x800000;

		public const uint WS_EX_DLGMODALFRAME = 1;
		public const uint WS_EX_WINDOWEDGE = 0x100;
		public const uint WS_EX_CLIENTEDGE = 0200;
		public const uint WS_EX_STATICEDGE = 0x20000;

		public const int SW_SHOWNOACTIVATE = 4;
		public const int SW_RESTORE = 9;

		public const int WM_EXITSIZEMOVE = 0x0232;

		[UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		public delegate bool EnumWindowsProc(int hwnd, IntPtr lParam);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool EnumThreadWindows(int dwThreadId, EnumWindowsProc lpfn, IntPtr lParam);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool EnumChildWindows(int hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool IsWindow(int hWnd);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool GetWindowInfo(int hwnd, ref WINDOWINFO pwi);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern int GetWindowLong(int hWnd, int nIndex);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern int SetWindowLong(int hWnd, int nIndex, uint dwNewLong);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool IsIconic(int hWnd);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool IsZoomed(int hWnd);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool ShowWindow(int hWnd, int nCmdShow);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern int GetForegroundWindow();

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern int SendMessage(int hWnd, int msg, int wParam, int lParam);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern int MonitorFromWindow(int hWnd, uint dwFlags);

		[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
		public static extern bool GetMonitorInfoA(int hMonitor, ref LPMONITORINFO lpmi);
	}

	/// <summary>
	/// RECT struct.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	struct RECT
	{
		public int left;
		public int top;
		public int right;
		public int bottom;

		public int Width { get { return right - left; } }
		public int Height { get { return bottom - top; } }

		public static void CopyRect(RECT rcSrc, ref RECT rcDest)
		{
			rcDest.left = rcSrc.left;
			rcDest.top = rcSrc.top;
			rcDest.right = rcSrc.right;
			rcDest.bottom = rcSrc.bottom;
		}
	}

	/// <summary>
	/// WINDOWINFO struct.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	struct WINDOWINFO
	{
		public uint cbSize;
		public RECT rcWindow;
		public RECT rcClient;
		public uint dwStyle;
		public uint dwExStyle;
		public uint dwWindowStatus;
		public uint cxWindowBorders;
		public uint cyWindowBorders;
		public ushort atomWindowType;
		public ushort wCreatorVersion;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct LPMONITORINFO
    {
		public uint cbSize;
		public RECT rcMonitor;
		public RECT rcWork;
		public uint dwFlags;
    }
}