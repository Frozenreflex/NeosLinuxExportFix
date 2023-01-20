# NeosLinuxExportFix
Fixes exporting files not working on the Linux Native build of NeosVR. On my machine, screenshots end up in ``$HOME/Pictures/Neos VR``, and other files go in ``$HOME/Neos VR``.

## For Neos Developers
```cs
  //LinuxPlatformConnector
  public void NotifyOfFile(string file, string name)
  {
  }

  public void NotifyOfScreenshot(
    World world,
    string file,
    ScreenshotType type,
    DateTime timestamp)
  {
  }
```
Really?
