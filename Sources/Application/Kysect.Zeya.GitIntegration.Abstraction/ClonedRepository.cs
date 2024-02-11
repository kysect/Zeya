using System.IO.Abstractions;

namespace Kysect.Zeya.GitIntegration.Abstraction;

public class ClonedRepository(string repositoryRootPath, IFileSystem fileSystem) : IClonedRepository
{
    public LocalRepositoryFileSystem FileSystem => new LocalRepositoryFileSystem(repositoryRootPath, fileSystem);

    public virtual string GetRepositoryName()
    {
        IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New(repositoryRootPath);
        return directoryInfo.Name;
    }
}