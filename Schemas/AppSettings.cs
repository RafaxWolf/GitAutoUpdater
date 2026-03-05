namespace GitAutoUpdater.Schemas
{
    public class AppSettings
    {
        public GitSettings GitSettings { get; set; }
        public int IntSeconds { get; set; }
        public string LogFile { get; set; }
    }
}
