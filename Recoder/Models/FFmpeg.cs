using Avalonia.Media;
using System;
using System.Diagnostics;
using System.IO;

namespace Recoder.Models
{
    public class FFmpeg
    {
        private Process? ffProcess;

        private readonly string savePath = string.Empty;

        private string fileName = string.Empty;

        public bool IsRecording { get; set; }

        public FFmpeg(string savePath)
        {
            this.savePath = savePath;
        }

        private static string GenCommand(string format, string input, string fileName, string color = "red", string text = "fasuo")
        {
            return $"-f {format} -i {input} -vf \"drawtext=fontsize=160:fontcolor={color}:text='{text}'\" -c:v libx264 -an {fileName}";
        }

        public void StartRecord()
        {
            if(IsRecording)
                return;

            try
            {
                ProcessStartInfo startInfo;
                fileName = Path.Combine(savePath, Guid.NewGuid().ToString() + ".mp4");

                if (OperatingSystem.IsWindows())
                {
                    startInfo = new("ffmpeg", GenCommand("gdigrab", "desktop", fileName));
                }
                else if (OperatingSystem.IsLinux())
                {
                    startInfo = new("ffmpeg", GenCommand("x11grab", ":0.0+0,0", fileName));
                }
                else if (OperatingSystem.IsMacOS())
                {
                    startInfo = new("ffmpeg", GenCommand("avfoundation", "0", fileName));
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
