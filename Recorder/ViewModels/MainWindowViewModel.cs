using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Recorder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private readonly Uploader uploader;

        private readonly IClassicDesktopStyleApplicationLifetime applicationLifetime;

        private readonly string savePath;

        private int uploaded = 0;

        /// <summary>
        /// Binding Properties
        /// </summary>

        public string RecordText => ffmpeg.IsRecording ? "结束录制" : "开始录制";

        public bool CanUpload => !ffmpeg.IsRecording;

        public string AlertText { get; private set; } = "0个文件已上传";

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

            applicationLifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime
                ?? throw new NullReferenceException();

            savePath = Path.Combine(appDataContext.CaptureResources, Guid.NewGuid().ToString());

            // Initialize require services
            ffmpeg = new FFmpeg(savePath);
            chromium = new Chromium(savePath);
            uploader = new(savePath);
            Initialize(savePath);

            // Bind Command
            SwitchRecordingCommand = ReactiveCommand.Create(SwitchRecording);
            OpenBrowserCommand = ReactiveCommand.Create(OpenBrowser);
            ScreenshotCommand = ReactiveCommand.Create(Screenshot);
            ExitCommand = ReactiveCommand.Create(Exit);
        }

        private static void Initialize(string savePath)
        {
            // 初始化资源目录
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            // 初始化退出事件
            //applicationLifetime.Exit += (o, e) => { Directory.Delete(savePath, true); };
        }

        private void SwitchRecording()
        {
            if (ffmpeg.IsRecording)
            {
                ffmpeg.StopRecord();
                Upload();
            }
            else
            {
                ffmpeg.StartRecord();
            }

            this.RaisePropertyChanged(nameof(CanUpload));
            this.RaisePropertyChanged(nameof(RecordText));
        }

        private void OpenBrowser()
        {
            chromium.LauncherPuppeteer();
        }

        private void Screenshot()
        {
            chromium.ScreenShot();
            Upload();
        }

        private void Upload()
        {
            // Upload files
            List<string> files = Directory.GetFiles(savePath).ToList();

            files.ForEach(async file =>
            {
                if(await uploader.Upload(file))
                {
                    AlertText = $"{++uploaded}个文件已上传";
                }

                this.RaisePropertyChanged(nameof(AlertText));
            });
        }

        private void Exit()
        {
            if (chromium.IsRunning)
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
