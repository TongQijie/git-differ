using GitDiffer.App.Git;

namespace GitDiffer.App.Abstractions
{
    public interface IGit
    {
        Commit[] GetTotalCommits();

        Change[] GetCommitDetail(string commitId);

        Commit GetFileHistory(string path, Commit beforeCommit);

        string GetFileContent(string commitId, string path);
    }
}
