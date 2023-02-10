using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Recorder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        private int fileCount = 0;

        private int uploadedCount = 0;

        private readonly object locker = new();

        /// <summary>
        /// Binding Properties
        /// </summary>

        public string RecordText => ffmpeg.IsRecording ? "结束录制" : "开始录制";

        public bool CanUpload => !ffmpeg.IsRecording;

        public string UploadText
        {
            get => $"{uploadedCount}/{fileCount} 个文件已上传";
        }

        public double UploadProgress
        {
            get => (double)fileCount / (double)uploadedCount * 100;
        }

        /// <summary>
        /// Binding Commands
        /// </summary>

        public ICommand SwitchRecordingCommand { get; set; }

        public ICommand OpenBrowserCommand { get; set; }

        public ICommand ScreenshotCommand { get; set; }

        public ICommand UploadCommand { get; set; }

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
            uploader = new();
            Initialize(savePath);

            // Bind Command
            SwitchRecordingCommand = ReactiveCommand.Create(SwitchRecording);
            OpenBrowserCommand = ReactiveCommand.Create(OpenBrowser);
            ScreenshotCommand = ReactiveCommand.Create(Screenshot);
            UploadCommand = ReactiveCommand.Create(Upload);
            ExitCommand = ReactiveCommand.Create(Exit);
        }

        private void Initialize(string savePath)
        {
            // 初始化资源目录
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            // 初始化退出事件
            applicationLifetime.Exit += (o, e) => { Directory.Delete(savePath); };
        }

        private void SwitchRecording()
        {
            if (ffmpeg.IsRecording)
            {
                ffmpeg.StopRecord();
                CheckFileCount();
            }
            else
            {
                ffmpeg.StartRecord();
            }

            this.RaisePropertyChanged(nameof(CanUpload));
            this.RaisePropertyChanged(nameof(RecordText));
            this.RaisePropertyChanged(nameof(UploadText));
        }

        private void OpenBrowser()
        {
            chromium.LauncherPuppeteer();
        }

        private async void Screenshot()
        {
            await chromium.ScreenShot();
            CheckFileCount();
            this.RaisePropertyChanged(nameof(UploadText));
        }

        private void CheckFileCount() => fileCount = Directory.GetFiles(savePath).Length;

        private async void Upload()
        {
            List<Task> tasks = new();

            Directory.GetFiles(savePath).ToList().ForEach(file =>
            {
                tasks.Add(Task.Run(async () =>
                {
                    if (await uploader.Upload(file))
                    {
                        lock(locker)
                        {
                            uploadedCount++;
                        }
                    }

                    this.RaisePropertyChanged(nameof(UploadText));
                    this.RaisePropertyChanged(nameof(UploadProgress));
                }));
            });

            Task.WaitAll(tasks.ToArray());
            await uploader.Report(uploadedCount);
            Exit();
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
