namespace Kysect.Zeya.GitIntegration.Abstraction;

public interface IClonedRepository
{
    string GetRepositoryName();

    string GetFullPath();
    bool Exists(string partialPath);
    string ReadAllText(string partialPath);
    void WriteAllText(string partialPath, string content);
}
