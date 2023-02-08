using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;

namespace Recorder.Models
{
    public class AppDataContext : ReactiveObject
    {
        private IClassicDesktopStyleApplicationLifetime applicationLifetime;

        public string AppPath { get; private set; } = string.Empty;

        public string CaptureResources { get; private set; } = string.Empty;

        public string UploadUrl { get; private set; } = string.Empty;

        public string ApiToken { get; private set; } = string.Empty;

        public AppDataContext()
        {
            applicationLifetime = App.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
                ?? throw new NullReferenceException();

            GenResourcePath();
            ParseArgs();
        }

        private void GenResourcePath()
        {
            // Get assembly path
            string hostName = Environment.ProcessPath ?? throw new NullReferenceException();

            if (OperatingSystem.IsWindows())
            {
                AppPath = hostName[..hostName.LastIndexOf("\\")];
            }
            else
            {
                AppPath = hostName[..hostName.LastIndexOf("/")];
            }

            CaptureResources = Path.Combine(AppPath, "CaptureResources");
        }

        private void ParseArgs()
        {
            // Check the upload url is not null or empty.
            string paraStr = applicationLifetime.Args?.FirstOrDefault() ?? string.Empty;
            string[] args = paraStr.ToLower().Replace("recorder://", string.Empty).Split("&");

            if (args.Length < 2)
                return;

            UploadUrl = args[0];
            ApiToken = args[1];

            if (string.IsNullOrEmpty(UploadUrl) || string.IsNullOrEmpty(ApiToken))
                applicationLifetime.Shutdown();
        }
    }
}
