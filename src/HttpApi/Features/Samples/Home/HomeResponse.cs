namespace Engrslan.Features.Samples.Home;

public class HomeResponse
{
    public string Message { get; set; } = string.Empty;
    public string Warning { get; set; } = string.Empty;
    public ServerInfo Server { get; set; } = new();
    public ApplicationInfo Application { get; set; } = new();
    
    public class ServerInfo
    {
        public string MachineName { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string Architecture { get; set; } = string.Empty;
        public int ProcessorCount { get; set; }
        public string DotNetVersion { get; set; } = string.Empty;
        public DateTime ServerTime { get; set; }
        public string TimeZone { get; set; } = string.Empty;
    }
    
    public class ApplicationInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public TimeSpan Uptime { get; set; }
        public string WorkingDirectory { get; set; } = string.Empty;
    }
}
