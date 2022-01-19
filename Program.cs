using Borderless1942.Win32Extensions;
using System.Diagnostics;

// Start Process
var processStartInfo = new ProcessStartInfo
{
	//FileName = @"BF1942.exe"
	FileName = @"S:\Battlefield 1942\BF1942.exe",
	WorkingDirectory = @"S:\Battlefield 1942\"
};
foreach (var arg in args)
{
	processStartInfo.ArgumentList.Add(arg);
}
var process = Process.Start(processStartInfo)!;
var keepAlive = true;
Console.WriteLine($"Process has started ({process.Id})");

// Wait for initial window handle
var window = await process.WaitForMainWindowAsync();
window.RemoveBorders();
using var eventHandle = window.OnResize(w =>
{
	var bounds = w.GetBounds();
	Console.WriteLine($"W: {bounds.Width}px   H: {bounds.Height}px");
});

Events.ProcessEvents();


// Keep itself alive until detected otherwise
//while (keepAlive)
//{
//MainLoop:
//	UpdateWindowPosition(window);
//	await Task.Delay(100);

//	if (process.HasExited)
//	{
//		var oldProcessId = process.Id;
//		var retryCount = 0;
//		while (retryCount < 2)
//		{
//			var matchingProcesses = Process.GetProcessesByName("BF1942");
//			if (matchingProcesses.Length == 1)
//			{
//				process = matchingProcesses[0];
//				Console.WriteLine($"Process has changed ({oldProcessId} -> {process.Id})");
//				window = await process.WaitForMainWindowAsync();
//				window.RemoveBorders();
//				goto MainLoop;
//			}
//			retryCount++;
//			await Task.Delay(TimeSpan.FromSeconds(1));
//		}
//		keepAlive = false;
//		Console.WriteLine($"Process has exited ({oldProcessId})");
//	}
//}

static void UpdateWindowPosition(Window window)
{
	var monitorBounds = window.GetCurrentMonitor().GetBounds();
	var windowBounds = window.GetBounds();
	var x = monitorBounds.Width / 2 - windowBounds.Width / 2;
	var y = monitorBounds.Height / 2 - windowBounds.Height / 2;
	window.SetPosition(x, y);
}