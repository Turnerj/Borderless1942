using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

namespace Borderless1942.Win32Extensions;

internal class BlockingEventRunner : IEventRunner
{
	public void ProcessEvents()
	{
		while (PInvoke.GetMessage(out var msg, default, 0, 0).Value > 0)
		{
			PInvoke.TranslateMessage(in msg);
			PInvoke.DispatchMessage(in msg);
		}
	}

	public SafeHandle AddEventHandler(EventRegistration registration)
	{
		var handle = PInvoke.SetWinEventHook(
			registration.EventMin,
			registration.EventMax,
			registration.Target.Win32Handle,
			(HWINEVENTHOOK hWinEventHook, uint @event, HWND hwnd, int idObject, int idChild, uint idEventThread, uint dwmsEventTime) =>
			{
				registration.Handler(new(@event, hwnd, (int)idEventThread));
			},
			(uint)registration.Target.ProcessId,
			(uint)registration.Target.ThreadId,
			PInvoke.WINEVENT_OUTOFCONTEXT
		);
		return new Win32EventSafeHandle(handle, ownsHandle: true);
	}

	public void RemoveEventHandler(nint eventHandle)
	{
		PInvoke.UnhookWinEvent(new((IntPtr)eventHandle));
	}
}
