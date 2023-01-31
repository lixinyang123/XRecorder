using ReactiveUI;
using System;
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
        public string Greeting { get; set; } = "ÍøÒ³È¡Ö¤";

        public ICommand DownloadFFmpegCommand { get; set; }

        public ICommand StartCaptureCommand { get; set; }

        public ICommand StopCaptureCommand { get; set; }

        public double DownloadFFmpegProgress { get; set; }

        private CancellationTokenSource? cancellationTokenSource;

        public MainWindowViewModel()
        {
            DownloadFFmpegCommand = ReactiveCommand.Create(DownloadFFmpeg);
            StartCaptureCommand = ReactiveCommand.Create(StartCapture);
            StopCaptureCommand = ReactiveCommand.Create(StopCapture);
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

        private void StartCapture()
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

                Greeting = "Start Capture";
            }
            catch (Exception ex)
            {
                Greeting = ex.Message;
            }
            finally
            {
                this.RaisePropertyChanged(nameof(Greeting));
            }
        }

        private void StopCapture()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();

            Greeting = "Stop Capture";
            this.RaisePropertyChanged(nameof(Greeting));
        }
    }
}
