using ReactiveUI;
using System.IO;

namespace Recorder.Models
{
    public class AppDataContext : ReactiveObject
    {
        public readonly string appPath = string.Empty;

        public readonly string captureResources = string.Empty;

        public AppDataContext()
        {
            // Get assembly path
            string hostName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            appPath = hostName.Substring(0, hostName.LastIndexOf("\\"));
            
            captureResources = Path.Combine(appPath, "CaptureResources");
        }
    }
}
