using System.Diagnostics;
using System;
using System.IO;

namespace Recoder.Models
{
    public class Chromium
    {
        private readonly string savePath = string.Empty;

        public Chromium(string savePath)
        {
            this.savePath = savePath;
        }

        public void LauncherPuppeteer()
        {

        }

        public static void Open()
        {
            string path;

            if (OperatingSystem.IsWindows())
            {
                path = Path.Combine("chrome-win", "chrome.exe");
            }
            else if (OperatingSystem.IsLinux())
            {
                path = Path.Combine("chrome-win", "chrome.exe");
            }
            else if (OperatingSystem.IsMacOS())
            {
                path = Path.Combine("chrome-win", "chrome.exe");
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            Process.Start(new ProcessStartInfo(path, "baidu.com")
            {
                CreateNoWindow = true
            });
        }
    }
}
