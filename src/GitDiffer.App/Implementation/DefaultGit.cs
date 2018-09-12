using GitDiffer.App.Abstractions;
using GitDiffer.App.Git;
using Guru.ExtensionMethod;
using Guru.DependencyInjection;
using Guru.DependencyInjection.Attributes;
using System;
using System.Linq;
using Guru.Utils;

namespace GitDiffer.App.Implementation
{
    [Injectable(typeof(IGit), Lifetime.Singleton)]
    internal class DefaultGit : IGit
    {
        public Change[] GetCommitDetail(string commitId)
        {
            var changes = new Change[0];
            using (var reader = ProcessUtils.ExecuteToStream(
                DependencyContainer.Resolve<IAppConfiguration>().GitPath, 
                $"show --pretty=\"\" --name-status {commitId}", 
                DependencyContainer.Resolve<IAppConfiguration>().LocalGitDirectory))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.SplitByChar('\t');
                    if (fields.Length >= 2)
                    {
                        if (fields[0].StartsWith("R", StringComparison.OrdinalIgnoreCase))
                        {
                            changes = changes.Append(new Change()
                            {
                                Action = "D",
                                Path = fields[1],
                            }).Append(new Change()
                            {
                                Action = "A",
                                Path = fields[2],
                            });
                        }
                        else if (fields[0] == "MM")
                        {
                            changes = changes.Append(new Change()
                            {
                                Action = "M",
                                Path = fields[1],
                            });
                        }
                        else
                        {
                            changes = changes.Append(new Change()
                            {
                                Action = fields[0],
                                Path = fields[1],
                            });
                        }
                    }
                }
            }

            return changes;
        }

        public string GetFileContent(string commitId, string path)
        {
            using (var reader = ProcessUtils.ExecuteToStream(
                DependencyContainer.Resolve<IAppConfiguration>().GitPath,
                $"show {commitId}:\"{path}\"",
                DependencyContainer.Resolve<IAppConfiguration>().LocalGitDirectory))
            {
                return reader.ReadToEnd();
            }
        }

        public Commit GetFileHistory(string path, Commit beforeCommit)
        {
            using (var reader = ProcessUtils.ExecuteToStream(
                DependencyContainer.Resolve<IAppConfiguration>().GitPath,
                $"log --name-status --before \"{beforeCommit.Date.AddSeconds(-1)}\" -- \"{path}\"",
                DependencyContainer.Resolve<IAppConfiguration>().LocalGitDirectory))
            {
                string line = reader.ReadLine();
                if (line == null)
                {
                    return null;
                }

                var commit = new Commit();
                if (line.StartsWith("commit", StringComparison.OrdinalIgnoreCase))
                {
                    commit.Id = line.Substring(line.IndexOf(' ')).Trim();
                }

                line = reader.ReadLine();
                if (line.StartsWith("Author", StringComparison.OrdinalIgnoreCase))
                {
                    commit.Author = line.Substring(line.IndexOf(' ')).Trim();
                }

                line = reader.ReadLine();
                if (line.StartsWith("Date", StringComparison.OrdinalIgnoreCase))
                {
                    //commit.Date = DateTime.Parse(line.Substring(line.IndexOf(' ')).Trim());
                }

                reader.ReadLine();
                commit.Subject = reader.ReadLine();

                return commit;
            }
        }

        public Commit[] GetTotalCommits()
        {
            var config = DependencyContainer.Resolve<IAppConfiguration>();
            
            var commits = new Commit[0];
            using (var reader = ProcessUtils.ExecuteToStream(
                DependencyContainer.Resolve<IAppConfiguration>().GitPath,
                $"log --after \"{config.StartTime.ToString("yyyy-MM-dd HH:mm:ss")}\" --before \"{(config.EndTime ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss")}\" --pretty=format:\"%H|%cI|%an|%s\" {config.BranchName}",
                DependencyContainer.Resolve<IAppConfiguration>().LocalGitDirectory))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.SplitByChar('|');
                    if (fields.Length >= 4)
                    {
                        commits = commits.Append(new Commit()
                        {
                            Id = fields[0],
                            Date = DateTime.Parse(fields[1]),
                            Author = fields[2],
                            Subject = string.Join("|", fields.Skip(3)),
                        });
                    }
                }
            }

            return commits;
        }
    }
}
