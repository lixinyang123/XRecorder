using PuppeteerSharp;
using System;
using System.IO;

namespace Recorder.Models
{
    public class Chromium
    {
        private readonly AppDataContext appDataContext;

        private readonly string chromiumPath = string.Empty;

        private readonly string savePath = string.Empty;

        private IBrowser? browser;

        private IPage? currentPage;

        public bool IsRunning => !browser?.IsClosed ?? false;

        public Chromium(string savePath)
        {
            appDataContext = App.Current?.DataContext as AppDataContext ?? throw new NullReferenceException();
            this.savePath = savePath;

            if (OperatingSystem.IsWindows())
            {
                chromiumPath = Path.Combine(appDataContext.AppPath, "chrome-win", "chrome.exe");
            }
            else if (OperatingSystem.IsLinux())
            {
                chromiumPath = Path.Combine(appDataContext.AppPath, "chrome-linux", "chrome");
            }
            else if (OperatingSystem.IsMacOS())
            {
                chromiumPath = Path.Combine(appDataContext.AppPath, "chrome-osx", "chrome");
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
                browser = null;
            };

            browser.TargetChanged += async (object? sender, TargetChangedArgs e) =>
            {
                currentPage = await e.Target.PageAsync();

                // Get the target IP
                //IPAddress[] addressList = Dns.GetHostEntry(new Uri(e.Target.Url).Host).AddressList;
            };

            IPage page = await browser.NewPageAsync();
            _ = page.GoToAsync("https://www.baidu.com");
        }

        public void ScreenShot()
        {
            currentPage?.ScreenshotAsync(Path.Combine(savePath, Guid.NewGuid().ToString() + ".png"));
        }

        public async void Close()
        {
            if (browser is null)
                return;

            await (browser?.CloseAsync() ?? throw new NullReferenceException());
            browser?.Dispose();
        }
    }
}
