using System;
using GitDiffer.App.Abstractions;
using Guru.DependencyInjection.Attributes;

namespace GitDiffer.App.Implementation
{
    [StaticFile(typeof(IAppConfiguration), "./Configuration/app.hjson", Format = "hjson")]
    internal class DefaultAppConfiguration : IAppConfiguration
    {
        public string GitPath { get; set; }

        public string LocalGitDirectory { get; set; }

        public string BranchName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string CompareTool { get; set; }

        public string CompareToolParams { get; set; }
    }
}
