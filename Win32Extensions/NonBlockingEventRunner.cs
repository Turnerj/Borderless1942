using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;

namespace Borderless1942.Win32Extensions;

internal class NonBlockingEventRunner : IEventRunner
{
	private readonly Thread EventThread;

	private const uint WM_EVENT_REGISTRATION = PInvoke.WM_APP + 1;
	private const uint WM_EVENT_REGISTRATION_COMPLETION = PInvoke.WM_APP + 2;
	private const uint WM_EVENT_UNREGISTER = PInvoke.WM_APP + 3;

	private readonly ConcurrentQueue<(int ThreadId, EventRegistration)> EventRegistrationQueue = new();

	public NonBlockingEventRunner()
	{
		EventThread = new(new ThreadStart(InternalEventLoop));
	}

	private void InternalEventLoop()
	{
		while (PInvoke.GetMessage(out var msg, default, 0, 0).Value > 0)
		{
			switch (msg.message)
			{
				case WM_EVENT_REGISTRATION:
					while (EventRegistrationQueue.TryDequeue(out var data))
					{
						var (threadId, registration) = data;
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
						PInvoke.PostThreadMessage((uint)threadId, WM_EVENT_REGISTRATION_COMPLETION, default, new(handle));
					}
					break;
				case WM_EVENT_UNREGISTER:
					PInvoke.UnhookWinEvent(new((IntPtr)msg.lParam.Value));
					break;
				default:
					PInvoke.TranslateMessage(in msg);
					PInvoke.DispatchMessage(in msg);
					break;
			}
		}
	}

	public void ProcessEvents() { }

	public SafeHandle AddEventHandler(EventRegistration registration)
	{
		EventRegistrationQueue.Enqueue((Environment.CurrentManagedThreadId, registration));
		PInvoke.PostThreadMessage((uint)EventThread.ManagedThreadId, WM_EVENT_REGISTRATION, default, default);
		if (PInvoke.GetMessage(out var msg, default, WM_EVENT_REGISTRATION_COMPLETION, WM_EVENT_REGISTRATION_COMPLETION).IsTrue())
		{
			return new Win32EventSafeHandle(msg.lParam.Value, ownsHandle: true);
		}
		return new Win32EventSafeHandle();
	}

	public void RemoveEventHandler(nint eventHandle)
	{
		PInvoke.PostThreadMessage((uint)EventThread.ManagedThreadId, WM_EVENT_UNREGISTER, default, new(eventHandle));
	}
}
