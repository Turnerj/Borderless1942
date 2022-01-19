# Borderless 1942

Provides borderless fullscreen support for Battlefield 1942.

## Why go borderless?

Basically I was having issues running the game in fullscreen on a machine of mine.
The game would run fine in the menu but would crash loading the map - simply by running it in a window fixed that.

Running in a window though was a bit crappy with the border and where it positioned itself on the screen.
Borderless 1942 removes the border and repositions the window based on the window size so it is always in the center.

BF1942 throws an extra curveball by changing process when you exit out of a map.
Borderless 1942 also tracks this and reapplies the border and positioning logic on the new process.

## Installation

1. [Download the latest release](https://github.com/Turnerj/Borderless1942/releases/latest) into the directory where `BF1942.exe` is located.
2. In `Mods\bf1942\Settings\VideoDefault.con`, set `renderer.setFullScreen` to `0`.
3. In `Mods\bf1942\Settings\Profiles\Custom\Video.con`, set `game.setGameDisplayMode` to the resolution of your monitor (eg. mine is set to `1920 1080 32 0`)
4. Run `Borderless1942.exe` and it will open BF1942, remove its borders and manage its position.
5. If you want to run the expansions, pass `+game {mod}` as arguments where `{mod}` is:
   - `XPack1` for the Road to Rome
   - `XPack2` for Secret Weapons of WWII

To make it easier to launch the expansions, create a shortcut to `Borderless1942.exe` with the following:
- Target: `"C:\path\to\bf1942\Borderless1942.exe" +game XPack2`
- Start In: `C:\path\to\bf1942\`

## Do I need .NET 6 installed to use Borderless 1942?

Nope! It is a self-contained executable - it contains everything it needs to run.

## Building from Source

You'll need the .NET 6 SDK but besides that, just run `build.bat` and it will build from source.