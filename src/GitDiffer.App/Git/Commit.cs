using System;

namespace GitDiffer.App.Git
{
    public class Commit
    {
        public int Index { get; set; }

        public string Id { get; set; }

        public DateTime Date { get; set; }

        public string Author { get; set; }

        public string Subject { get; set; }
    }
}