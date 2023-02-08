using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Recorder.Models;
using System;
using System.IO;
using System.Windows.Input;

namespace Recorder.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Private field
        /// </summary>

        private readonly AppDataContext appDataContext;
        
        private readonly FFmpeg ffmpeg;

        private readonly Chromium chromium;

        private readonly IClassicDesktopStyleApplicationLifetime applicationLifetime;

        /// <summary>
        /// Binding Properties
        /// </summary>

        public string RecordText => ffmpeg.IsRecording ? "结束录制" : "开始录制";

        /// <summary>
        /// Binding Commands
        /// </summary>

        public ICommand SwitchRecordingCommand { get; set; }

        public ICommand OpenBrowserCommand { get; set; }

        public ICommand ScreenshotCommand { get; set; }

        public ICommand ExitCommand { get; set; }

        public MainWindowViewModel()
        {
            appDataContext = App.Current?.DataContext as AppDataContext ?? throw new NullReferenceException();
            string savePath = Path.Combine(appDataContext.captureResources, Guid.NewGuid().ToString());

            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            ffmpeg = new FFmpeg(savePath);
            chromium = new Chromium(savePath);

            applicationLifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
                ?? throw new NullReferenceException();

            SwitchRecordingCommand = ReactiveCommand.Create(SwitchRecording);
            OpenBrowserCommand = ReactiveCommand.Create(OpenBrowser);
            ScreenshotCommand = ReactiveCommand.Create(Screenshot);
            ExitCommand = ReactiveCommand.Create(Exit);
        }

        private void SwitchRecording()
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
            chromium.LauncherPuppeteer();
            //chromium.Open();
        }

        private void Screenshot()
        {
            chromium.ScreenShot();
        }

        private void Exit()
        {
            if(chromium.IsRunning)
            {
                chromium.Close();
            }

            if (ffmpeg.IsRecording)
            {
                ffmpeg.StopRecord();
            }

            applicationLifetime.Shutdown();
        }
    }
}
