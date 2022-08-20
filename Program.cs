using Borderless1942;
using System.Diagnostics;

// Start Process
var processStartInfo = new ProcessStartInfo
{
	FileName = @"BF1942.exe"
};
foreach (var arg in args)
{
	processStartInfo.ArgumentList.Add(arg);
}
var process = Process.Start(processStartInfo)!;
var keepAlive = true;
Console.WriteLine($"[Borderless1942]: [{DateTime.Now:yyyy-MM-dd - hh:mm:ss tt}] [BF1942 Process Has Started] [{process.Id}]");

// Wait for initial window handle
var window = await process.WaitForMainWindowAsync();
window.RemoveBorders();

// Keep itself alive until detected otherwise
while (keepAlive)
{
MainLoop:
	UpdateWindowPosition(window);
	await Task.Delay(100);

	if (process.HasExited)
	{
		var oldProcessId = process.Id;
		var retryCount = 0;
		while (retryCount < 3)
		{
			var matchingProcesses = Process.GetProcessesByName("BF1942");
			if (matchingProcesses.Length == 1)
			{
				process = matchingProcesses[0];
				if (oldProcessId == process.Id)
				{
					Console.WriteLine($"[Borderless1942]: [{DateTime.Now:yyyy-MM-dd - hh:mm:ss tt}] [BF1942 Process Has Exited] [Closing Program] [{oldProcessId} -> {process.Id}]");
					keepAlive = false;
					Environment.Exit(0);
				}
				Console.WriteLine($"[Borderless1942]: [{DateTime.Now:yyyy-MM-dd - hh:mm:ss tt}] [BF1942 Process Has Changed] [{oldProcessId} -> {process.Id}]");
				window = await process.WaitForMainWindowAsync();
				window.RemoveBorders();
				goto MainLoop;
			}
			retryCount++;
			await Task.Delay(TimeSpan.FromSeconds(1));
		}
	}
}

static void UpdateWindowPosition(Window window)
{
	var monitorBounds = window.GetCurrentMonitor().GetBounds();
	var windowBounds = window.GetBounds();
	var x = monitorBounds.Width / 2 - windowBounds.Width / 2;
	var y = monitorBounds.Height / 2 - windowBounds.Height / 2;
	window.SetPosition(x, y);
}
