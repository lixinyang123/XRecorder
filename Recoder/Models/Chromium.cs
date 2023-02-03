using System.Diagnostics;
using System;
using System.IO;
using PuppeteerSharp;
using System.Linq;

namespace Recoder.Models
{
    public class Chromium
    {
        private readonly string chromiumPath = string.Empty;

        private readonly string savePath = string.Empty;

        private IBrowser? browser;

        public Chromium(string savePath)
        {
            this.savePath = savePath;

            if (OperatingSystem.IsWindows())
            {
                chromiumPath = Path.Combine("chrome-win", "chrome.exe");
            }
            else if (OperatingSystem.IsLinux())
            {
                chromiumPath = Path.Combine("chrome-win", "chrome");
            }
            else if (OperatingSystem.IsMacOS())
            {
                chromiumPath = Path.Combine("chrome-win", "chrome");
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public async void LauncherPuppeteer()
        {
            if (browser is not null)
                return;

            browser = await Puppeteer.LaunchAsync(new LaunchOptions 
            { 
                Headless = false,
                ExecutablePath = chromiumPath,
                DefaultViewport = null
            });

            browser.Closed += (object? sender, EventArgs e) =>
            {
                browser.Dispose();
                browser = null;
            };

            IPage page = await browser.NewPageAsync();
            await page.GoToAsync("https://www.baidu.com");
        }

        public async void ScreenShot()
        {
            try
            {
                IPage[] pages = await (browser?.PagesAsync() ?? throw new NullReferenceException());
                pages.ToList().ForEach(async page =>
                {
                    await page.ScreenshotAsync(Path.Combine(savePath, Guid.NewGuid().ToString() + ".png"));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Open()
        {
            Process.Start(new ProcessStartInfo(chromiumPath, "baidu.com")
            {
                CreateNoWindow = true
            });
        }

        public async void Close()
        {
            await (browser?.CloseAsync() ?? throw new NullReferenceException());
        }
    }
}
