using Avalonia.Controls;
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
        public string RecordText { get; set; } = "Start";

        public ICommand DownloadFFmpegCommand { get; set; }

        public ICommand SwitchCaptureCommand { get; set; }

        public double DownloadFFmpegProgress { get; set; }

        private CancellationTokenSource? cancellationTokenSource;

        public MainWindowViewModel()
        {
            DownloadFFmpegCommand = ReactiveCommand.Create(DownloadFFmpeg);
            SwitchCaptureCommand = ReactiveCommand.Create(SwitchCapture);
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
            if(RecordText == "Stop")
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();

                RecordText = "Start";
                this.RaisePropertyChanged(nameof(RecordText));
                return;
            }

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

                RecordText = "Stop";
                this.RaisePropertyChanged(nameof(RecordText));
            }
            catch (Exception ex)
            {
                _ = new Window() { Content = new TextBlock() { Text = ex.Message } };
            }
        }
    }
}
