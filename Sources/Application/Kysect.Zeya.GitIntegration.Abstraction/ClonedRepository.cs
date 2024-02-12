using System.IO.Abstractions;

namespace Kysect.Zeya.GitIntegration.Abstraction;

public class ClonedRepository(string repositoryRootPath, IFileSystem fileSystem) : IClonedRepository
{
    public LocalRepositoryFileSystem FileSystem { get; } = new LocalRepositoryFileSystem(repositoryRootPath, fileSystem);
    public LocalRepositorySolutionManager SolutionManager { get; } = new LocalRepositorySolutionManager(repositoryRootPath, fileSystem);

    public virtual string GetRepositoryName()
    {
        IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New(repositoryRootPath);
        return directoryInfo.Name;
    }
}