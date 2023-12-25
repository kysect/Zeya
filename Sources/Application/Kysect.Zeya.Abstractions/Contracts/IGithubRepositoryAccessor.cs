using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGithubRepositoryAccessor
{
    GithubRepository Repository { get; }

    string GetFullPath();
    bool Exists(string partialPath);
    string ReadAllText(string partialPath);
    void WriteAllText(string partialPath, string content);
    string GetWorkflowPath(string workflowName);
}
