using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace Borderless1942.Win32Extensions;

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
