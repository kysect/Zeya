namespace Kysect.Zeya.Abstractions.Contracts;

public interface IClonedRepository
{
    string GetFullPath();
    bool Exists(string partialPath);
    string ReadAllText(string partialPath);
    void WriteAllText(string partialPath, string content);
    string GetWorkflowPath(string workflowName);
}
