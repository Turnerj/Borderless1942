using Borderless1942;
using System.Diagnostics;

Console.WriteLine("Borderless1942: Runs BF1942 in a Borderless Window");
Console.WriteLine(string.Empty);

var windowsAnalyzer = new PlatformKit.Windows.WindowsAnalyzer();
var windowsVersion = windowsAnalyzer.GetWindowsVersionToEnum();
if (Environment.Is64BitOperatingSystem && windowsVersion > PlatformKit.Windows.WindowsVersion.Win8_1)
{
    var bf1942ExeName = "BF1942.exe";
    var processName = "Borderless1942";
    var borderless1942ExecutingPath = string.Empty;
    var bf1942ExePath = string.Empty;

    var foundProcess = Process.GetProcessesByName(processName).FirstOrDefault();
    if (foundProcess?.MainModule != null)
    {
        borderless1942ExecutingPath = Path.GetDirectoryName(foundProcess.MainModule.FileName);
    }

    if (!string.IsNullOrWhiteSpace(borderless1942ExecutingPath) && !string.IsNullOrWhiteSpace(bf1942ExeName))
    {
        bf1942ExePath = Path.Combine(borderless1942ExecutingPath, bf1942ExeName);
    }

    if (File.Exists(bf1942ExePath))
    {
        // Start Process
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "BF1942.exe"
        };
        foreach (var arg in args)
        {
            processStartInfo.ArgumentList.Add(arg);
        }
        var process = Process.Start(processStartInfo)!;
        var keepAlive = true;
        Console.WriteLine($"BF1942 Process Has Started [{process.Id}]");

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
                        if (process.HasExited)
                        {
                            break;
                        }
                        Console.WriteLine($"BF1942 Process Has Changed [{oldProcessId} -> {process.Id}]");
                        window = await process.WaitForMainWindowAsync();
                        window.RemoveBorders();
                        goto MainLoop;
                    }
                    retryCount++;
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                keepAlive = false;
                Console.WriteLine("BF1942 Process Has Exited");
            }
        }
    }
    else
    {
        Console.WriteLine($"Could not find {bf1942ExeName} in {borderless1942ExecutingPath}");
        Console.WriteLine("Please place Borderless1942.exe in the same folder as BF1942.exe");
        Console.WriteLine(string.Empty);
        Console.WriteLine("Press any key to Close the Program");
        Console.ReadKey();
    }
}
else
{
    Console.WriteLine("This Program is Designed for PCs running Windows 10 64bit or Higher");
    Console.WriteLine(string.Empty);
    Console.WriteLine("Press any key to Close the Program");
    Console.ReadKey();
}

return;

static void UpdateWindowPosition(Window window)
{
    var monitorBounds = window.GetCurrentMonitor().GetBounds();
    var windowBounds = window.GetBounds();
    var x = monitorBounds.Width / 2 - windowBounds.Width / 2;
    var y = monitorBounds.Height / 2 - windowBounds.Height / 2;
    window.SetPosition(x, y);
}
