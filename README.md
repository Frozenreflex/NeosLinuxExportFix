# NeosLinuxExportFix
Fixes exporting files not working on the Linux Native build of NeosVR

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
