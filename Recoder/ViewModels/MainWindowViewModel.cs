using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
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

        private const string BROWSER_PATH = "browser.exe";

        private const string VIDEO_PATH = "DesktopCapture";

        private Process? ffProcess;

        private bool isRecording = false;

        private readonly IClassicDesktopStyleApplicationLifetime applicationLifetime;

        /// <summary>
        /// Binding Properties
        /// </summary>

        public string RecordText => isRecording ? "Stop" : "Start";

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
                ProcessStartInfo startInfo = new();
                string fileName = Path.Combine(VIDEO_PATH, Guid.NewGuid().ToString() + ".mp4");

                if(System.OperatingSystem.IsWindows())
                {
                    startInfo = new("ffmpeg", $"-f gdigrab -i desktop {fileName}");
                }
                else if(System.OperatingSystem.IsLinux())
                {
                    startInfo = new("ffmpeg", $"-f x11grab -i :0.0+0,0 {fileName}");
                }
                else if(System.OperatingSystem.IsMacOS())
                {
                    startInfo = new("ffmpeg", $"-f avfoundation -i 0 {fileName}");
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }

                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardInput = true;

                ffProcess = Process.Start(startInfo);
                isRecording = true;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                isRecording = false;
            }
        }

        private void StopRecord()
        {
            if(System.OperatingSystem.IsWindows())
            {
                ffProcess?.StandardInput.Write('q');
            }
            else
            {
                ffProcess?.CloseMainWindow();
            }

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

            // У���ļ���ϣ����ֹ�۸�
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
