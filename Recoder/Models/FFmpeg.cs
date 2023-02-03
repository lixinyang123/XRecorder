using System;
using System.Diagnostics;
using System.IO;

namespace Recoder.Models
{
    public class FFmpeg
    {
        private Process? ffProcess;

        private readonly string savePath = string.Empty;

        public bool IsRecording { get; set; }

        public FFmpeg(string savePath)
        {
            this.savePath = savePath;
        }

        public void StartRecord()
        {
            if(IsRecording)
                return;

            try
            {
                ProcessStartInfo startInfo;
                string fileName = Path.Combine(savePath, Guid.NewGuid().ToString() + ".mp4");

                if (OperatingSystem.IsWindows())
                {
                    startInfo = new("ffmpeg", $"-f gdigrab -i desktop {fileName}");
                }
                else if (OperatingSystem.IsLinux())
                {
                    startInfo = new("ffmpeg", $"-f x11grab -i :0.0+0,0 {fileName}");
                }
                else if (OperatingSystem.IsMacOS())
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
                IsRecording = true;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                IsRecording = false;
            }
        }

        public void StopRecord()
        {
            if(!IsRecording)
                return;

            if (OperatingSystem.IsWindows())
            {
                ffProcess?.StandardInput.Write('q');
            }
            else
            {
                ffProcess?.CloseMainWindow();
            }

            ffProcess?.WaitForExit();
            ffProcess?.Dispose();

            IsRecording = false;
        }
    }
}
