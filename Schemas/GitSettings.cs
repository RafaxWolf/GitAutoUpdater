
namespace GitAutoUpdater.Schemas
{
    public class GitSettings
    {
        public string RepoUrl { get; set; }
        public string Branch { get; set; }
        public string LocalPath { get; set; }
        public bool UseDedicatedFolder { get; set; } = false;
    }
}
