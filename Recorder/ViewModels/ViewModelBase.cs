using ReactiveUI;
using System.IO;

namespace Recorder.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public readonly string captureResources = string.Empty;

        public ViewModelBase()
        {
            // Get assembly path
            string hostName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            hostName = hostName.Substring(0, hostName.LastIndexOf("\\"));
            captureResources = Path.Combine(hostName, "CaptureResources");
        }
    }
}
