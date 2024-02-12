namespace Kysect.Zeya.LocalRepositoryAccess;

public interface ILocalRepository
{
    string GetRepositoryName();

    LocalRepositoryFileSystem FileSystem { get; }
    LocalRepositorySolutionManager SolutionManager { get; }
}
