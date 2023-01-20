using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using BaseX;
using CodeX;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;
using MimeDetective;

namespace NeosLinuxExportFix;

public class LinuxExportFix : NeosMod
{
    public static readonly SemaphoreSlim ScreenshotSemaphore = new SemaphoreSlim(1, 1);
    private static LinuxPlatformConnector _connector;
    private static FieldInfo _field;
    public override string Name => "LinuxExportFix";
    public override string Author => "Fro Zen";
    public override string Version => "1.0.0";

    public static bool KeepScreenshotFormat()
    {
        _connector ??= Engine.Current.PlatformInterface.GetConnectors<LinuxPlatformConnector>().First() ??
                       throw new Exception();
        _field ??= typeof(LinuxPlatformConnector).GetField("keepOriginalScreenshotFormat",
            BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception();
        return (bool) _field.GetValue(_connector);
    }
    public override void OnEngineInit()
    {
        var harmony = new Harmony("LinuxExportFixHarmony");
        if (Engine.Current.Platform != Platform.Linux)
            Msg("Platform isn't Linux, you shouldn't be using this mod!");
        else
            harmony.PatchAll();
    }
}
[HarmonyPatch(typeof(PlatformInterface))]
public class PlatformInterfacePatch
{
    //all methods here are copied from the windows platform connector and modified to work as a mod
    //patching the linux connector seems to cause mono crashes so i'm patching the platform interface itself
    [HarmonyPrefix]
    [HarmonyPatch("NotifyOfFile")]
    public static bool NotifyOfFile(string file, string name)
    {
        Engine.Current.GlobalCoroutineManager.StartTask(async () =>
        {
            await default(ToBackground);
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            folderPath = Path.Combine(folderPath, "Neos VR");
            Directory.CreateDirectory(folderPath);
            var num = 1;
            string text2;
            do
            {
                var extension = Path.GetExtension(name);
                var text = Path.GetFileNameWithoutExtension(name);
                if (num > 1) text = text + "-" + num;
                text2 = Path.Combine(folderPath, text + extension);
                num++;
            }
            while (File.Exists(text2));
            File.Copy(file, text2);
            File.SetAttributes(text2, FileAttributes.Normal);
        });
        return true;
    }
    [HarmonyPrefix]
    [HarmonyPatch("NotifyOfScreenshot")]
    public static bool NotifyOfScreenshot(World world, string file, ScreenshotType type, DateTime time)
    {
        var b = LinuxExportFix.KeepScreenshotFormat();
        Engine.Current.GlobalCoroutineManager.StartTask(async () =>
        {
            await default(ToBackground);
            var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            pictures = Path.Combine(pictures, "Neos VR");
            Directory.CreateDirectory(pictures);
            var filename = time.ToString("yyyy-MM-dd HH.mm.ss");
            var extension = b ? Path.GetExtension(file) : ".jpg";
            if (string.IsNullOrWhiteSpace(extension))
            {
                var fileType = new FileInfo(file).GetFileType();
                if (fileType != null) extension = "." + fileType.Extension;
            }
            await LinuxExportFix.ScreenshotSemaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
            var num = 1;
            try
            {
                string text2;
                do
                {
                    var text = filename;
                    if (num > 1) text += $" ({num})";
                    text2 = Path.Combine(pictures, text + extension);
                    num++;
                }
                while (File.Exists(text2));
                if (b)
                {
                    File.Copy(file, text2);
                    File.SetAttributes(text2, FileAttributes.Normal);
                }
                else
                    TextureEncoder.ConvertToJPG(file, text2);
            }
            catch (Exception ex)
            {
                UniLog.Error("Exception saving screenshot:\n" + ex);
            }
            finally
            {
                LinuxExportFix.ScreenshotSemaphore.Release();
            }
        });
        return true;
    }
}