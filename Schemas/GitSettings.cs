using System;
using System.Collections.Generic;
using System.Text;

namespace GitAutoUpdater.Schemas
{
    public class GitSettings
    {
        public string RepoUrl { get; set; }
        public string Branch { get; set; }
        public string LocalPath { get; set; }
    }
}
