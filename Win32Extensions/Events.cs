using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace Borderless1942.Win32Extensions;

internal static class Events
{
	private static readonly IEventRunner Runner = new NonBlockingEventRunner();

	public static void ProcessEvents() => Runner.ProcessEvents();

	public static SafeHandle AddEventHandler(EventRegistration registration) => Runner.AddEventHandler(registration);

	/// <summary>
	/// Adds an event listener.
	/// </summary>
	/// <param name="eventMin">
	/// Specifies the <a href="https://docs.microsoft.com/windows/desktop/WinAuto/event-constants">event constant</a> for the lowest event value in the range of events that are handled by the hook function.
	/// </param>
	/// <param name="eventMax">
	/// Specifies the <a href="https://docs.microsoft.com/windows/desktop/WinAuto/event-constants">event constant</a> for the highest event value in the range of events that are handled by the hook function.
	/// </param>
	/// <param name="handler">
	/// (eventId, windowHandle, threadId)
	/// </param>
	/// <returns></returns>
	public static SafeHandle AddEventHandler(uint eventMin, uint eventMax, EventTarget target, Action<EventArgs> handler)
	{
		return AddEventHandler(new(eventMin, eventMax, target, handler));
	}

	/// <summary>
	/// Adds an event listener.
	/// </summary>
	/// <param name="eventId">
	/// Specifies the <a href="https://docs.microsoft.com/windows/desktop/WinAuto/event-constants">event constant</a> that are handled by the hook function.
	/// </param>
	/// <param name="handler">
	/// (eventId, windowHandle, threadId)
	/// </param>
	/// <returns></returns>
	public static SafeHandle AddEventHandler(uint eventId, EventTarget target, Action<EventArgs> handler) => AddEventHandler(eventId, eventId, target, handler);

	public static void RemoveEventHandler(nint eventHandle) => Runner.RemoveEventHandler(eventHandle);
}

internal interface IEventRunner
{
	public SafeHandle AddEventHandler(EventRegistration registration);
	public void RemoveEventHandler(nint eventHandle);
	public void ProcessEvents();
}

internal readonly record struct EventRegistration(uint EventMin, uint EventMax, EventTarget Target, Action<EventArgs> Handler);

/// <summary>
/// Provides information on the target of the event.
/// </summary>
/// <param name="Handle"></param>
/// <param name="ProcessId">Specifies the ID of the process from which the hook function receives events. Specify zero (0) to receive events from all processes on the current desktop.</param>
/// <param name="ThreadId">Specifies the ID of the thread from which the hook function receives events. If this parameter is zero, the hook function is associated with all existing threads on the current desktop.</param>
internal readonly record struct EventTarget(nint Handle, int ProcessId = 0, int ThreadId = 0)
{
	public static readonly EventTarget NoSpecificTarget = default;

	public HINSTANCE Win32Handle => new((IntPtr)Handle);
}

internal readonly record struct EventArgs(uint EventId, HWND Handle, int ThreadId);
