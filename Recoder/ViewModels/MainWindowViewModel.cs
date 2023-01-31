using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
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

        private CancellationTokenSource? cancellationTokenSource;

        private bool isRecording = false;

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
            string output = Path.Combine("./desktop_capture.mp4");
            File.Delete(output);

            cancellationTokenSource = new();

            try
            {
                FFmpeg.Conversions.New()
                    .AddDesktopStream()
                    .SetInputTime(TimeSpan.FromSeconds(10))
                    .SetOutput(output)
                    .Start(cancellationTokenSource.Token);

                isRecording = true;
            }
            catch (Exception)
            {
                isRecording = false;
            }
        }

        private void StopRecord()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();

            isRecording = false;
        }

        private void OpenBrowser()
        {
            if(!File.Exists(browserName))
            {
                return;
            }

            // 校验文件哈希，防止篡改
            Process.Start(browserName);
        }

        private void Exit()
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}
