using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Recoder.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace Recoder.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Private field
        /// </summary>
        private readonly FFmpeg ffmpeg;

        private readonly IClassicDesktopStyleApplicationLifetime applicationLifetime;

        /// <summary>
        /// Binding Properties
        /// </summary>

        public string RecordText => ffmpeg.IsRecording ? "Stop" : "Start";

        /// <summary>
        /// Binding Commands
        /// </summary>

        public ICommand SwitchCaptureCommand { get; set; }

        public ICommand OpenBrowserCommand { get; set; }

        public ICommand ExitCommand { get; set; }

        public MainWindowViewModel()
        {
            ffmpeg = new FFmpeg();

            applicationLifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
                ?? throw new NullReferenceException();

            SwitchCaptureCommand = ReactiveCommand.Create(SwitchCapture);
            OpenBrowserCommand = ReactiveCommand.Create(OpenBrowser);
            ExitCommand = ReactiveCommand.Create(Exit);
        }

        private void SwitchCapture()
        {
            if (ffmpeg.IsRecording)
            {
                ffmpeg.StopRecord();
            }
            else
            {
                ffmpeg.StartRecord();
            }

            this.RaisePropertyChanged(nameof(RecordText));
        }

        private void OpenBrowser()
        {
            string path;

            if(OperatingSystem.IsWindows())
            {
                path = Path.Combine("chrome-win", "chrome.exe");
            }
            else if(OperatingSystem.IsLinux())
            {
                path = Path.Combine("chrome-win", "chrome.exe");
            }
            else if(OperatingSystem.IsMacOS())
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

        private void Exit()
        {
            if (ffmpeg.IsRecording)
            {
                ffmpeg.StopRecord();
            }

            applicationLifetime.Shutdown();
        }
    }
}
