using System.IO.Abstractions;

namespace Kysect.Zeya.LocalRepositoryAccess;

public class LocalRepository(string repositoryRootPath, IFileSystem fileSystem) : ILocalRepository
{
    public LocalRepositoryFileSystem FileSystem { get; } = new LocalRepositoryFileSystem(repositoryRootPath, fileSystem);
    public LocalRepositorySolutionManager SolutionManager { get; } = new LocalRepositorySolutionManager(repositoryRootPath, fileSystem);

    public virtual string GetRepositoryName()
    {
        IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New(repositoryRootPath);
        return directoryInfo.Name;
    }
}