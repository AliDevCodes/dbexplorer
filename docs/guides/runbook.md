# Runbook

How to build, test, run and ship FastDbExplorer. Moved from `docs/RUNBOOK.md` on 2026-10-25; content kept, plus the crash-log note.

**Build and run (Windows, .NET 10 SDK):** `dotnet build FastDbExplorer.slnx`, then `dotnet run --project src/FastDbExplorer.Wpf`.

**Tests (xUnit):** `dotnet test`. A test is a method marked `[Fact]` that checks one behaviour; `[Theory]` runs the same check with many inputs.

**Offline package:** double-click `publish.cmd` -> `publish\FastDbExplorer\FastDbExplorer.exe` (and a ZIP). Self-contained win-x64: no .NET needed on the target PC.

**Persian font:** optional. Copy Vazirmatn `.ttf` files into `src/FastDbExplorer.Wpf/Assets/Fonts/` and rebuild.

**Maps:** run `setup-map-assets.cmd` once (downloads MapLibre into `src/FastDbExplorer.Wpf/Assets/Map`). `publish.cmd` does this automatically if the files are missing. Target PCs need the Edge WebView2 Runtime.

**Crash log:** unhandled exceptions and WebView2 process failures are written to `%LocalAppData%\FastDbExplorer\logs\crash.log`. A stack overflow kills the process and is not written there: use the Visual Studio Call Stack window in a Debug build (copy the first 40 lines).
