using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace Borderless1942.Win32Extensions;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Rectangle(int Left, int Top, int Right, int Bottom)
{
	public int Width => Right - Left;
	public int Height => Bottom - Top;
	
	internal static Rectangle From(RECT rectangle) => Unsafe.As<RECT, Rectangle>(ref rectangle);
}
