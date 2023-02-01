using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace Recoder.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Private field
        /// </summary>

        private const string browserName = "browser.exe";

        private Process? ffProcess;

        private bool isRecording = false;

        private readonly IClassicDesktopStyleApplicationLifetime applicationLifetime;

        /// <summary>
        /// Binding Properties
        /// </summary>

        public string RecordText => isRecording ? "Stop" : "Start";

        public double DownloadFFmpegProgress { get; set; }

        /// <summary>
        /// Binding Commands
        /// </summary>

        public ICommand DownloadFFmpegCommand { get; set; }

        public ICommand SwitchCaptureCommand { get; set; }

        public ICommand OpenBrowserCommand { get; set; }

        public ICommand ExitCommand { get; set; }

        public MainWindowViewModel()
        {
            applicationLifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
                ?? throw new NullReferenceException();

            DownloadFFmpegCommand = ReactiveCommand.Create(DownloadFFmpeg);
            SwitchCaptureCommand = ReactiveCommand.Create(SwitchCapture);
            OpenBrowserCommand = ReactiveCommand.Create(OpenBrowser);
            ExitCommand = ReactiveCommand.Create(Exit);
        }

        private async Task DownloadFFmpeg()
        {
            Progress<ProgressInfo> progress = new();

            progress.ProgressChanged += (object? sender, ProgressInfo e) =>
            {
                DownloadFFmpegProgress = (double)e.DownloadedBytes / e.TotalBytes * 100;
                this.RaisePropertyChanged(nameof(DownloadFFmpegProgress));
            };

            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, progress);
        }

        private void SwitchCapture()
        {
            if (isRecording)
            {
                StopRecord();
            }
            else
            {
                StartRecord();
            }

            this.RaisePropertyChanged(nameof(RecordText));
        }

        private void StartRecord()
        {
            string output = Path.Combine("./1Capture.mp4");
            File.Delete(output);

            try
            {
                string command = FFmpeg.Conversions.New()
                    .AddDesktopStream(null, 30)
                    .SetOutput(output)
                    .Build();

                ffProcess = Process.Start(new ProcessStartInfo("ffmpeg.exe", command)
                {
                    CreateNoWindow = true,
                    RedirectStandardInput = true
                });

                isRecording = true;
            }
            catch (Exception)
            {
                isRecording = false;
            }
        }

        private void StopRecord()
        {
            ffProcess?.StandardInput.Write('q');
            ffProcess?.WaitForExit();
            ffProcess?.Dispose();

            isRecording = false;
        }

        private void OpenBrowser()
        {
            if (!File.Exists(browserName))
            {
                return;
            }

            // 校验文件哈希，防止篡改
            Process.Start(browserName);
        }

        private void Exit()
        {
            if (isRecording)
            {
                StopRecord();
            }

            applicationLifetime.Shutdown();
        }
    }
}
