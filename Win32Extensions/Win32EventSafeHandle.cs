using System.Runtime.InteropServices;

namespace Borderless1942.Win32Extensions;

public class Win32EventSafeHandle : SafeHandle
{
	private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

	public Win32EventSafeHandle() : base(INVALID_HANDLE_VALUE, true) { }
	internal Win32EventSafeHandle(nint preexistingHandle, bool ownsHandle = true) : base(INVALID_HANDLE_VALUE, ownsHandle)
	{
		SetHandle(preexistingHandle);
	}

	public override bool IsInvalid => handle == default || handle == INVALID_HANDLE_VALUE;

	protected override bool ReleaseHandle()
	{
		Events.RemoveEventHandler((nint)handle);
		return true;
	}
}