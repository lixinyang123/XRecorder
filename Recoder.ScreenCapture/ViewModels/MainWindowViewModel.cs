using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Xabe.FFmpeg;

namespace Recoder.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Private field
        /// </summary>

        private const string BROWSER_PATH = "browser.exe";

        private const string FFMPEG_PATH = "ffmpeg.exe";

        private const string VIDEO_PATH = "DesktopCapture";

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

        public ICommand SwitchCaptureCommand { get; set; }

        public ICommand OpenBrowserCommand { get; set; }

        public ICommand ExitCommand { get; set; }

        public MainWindowViewModel()
        {
            applicationLifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
                ?? throw new NullReferenceException();

            SwitchCaptureCommand = ReactiveCommand.Create(SwitchCapture);
            OpenBrowserCommand = ReactiveCommand.Create(OpenBrowser);
            ExitCommand = ReactiveCommand.Create(Exit);

            if (!Directory.Exists(VIDEO_PATH))
                Directory.CreateDirectory(VIDEO_PATH);

            if (File.Exists(FFMPEG_PATH))
                DownloadFFmpegProgress = 100;
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
            try
            {
                string command = FFmpeg.Conversions.New()
                    .AddDesktopStream(null, 30)
                    .SetOutput(Path.Combine(VIDEO_PATH, Guid.NewGuid().ToString() + ".mp4"))
                    .Build();

                ffProcess = Process.Start(new ProcessStartInfo(FFMPEG_PATH, command)
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
            if (!File.Exists(BROWSER_PATH))
            {
                return;
            }

            // 校验文件哈希，防止篡改
            Process.Start(new ProcessStartInfo(BROWSER_PATH)
            {
                CreateNoWindow = true
            });
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
