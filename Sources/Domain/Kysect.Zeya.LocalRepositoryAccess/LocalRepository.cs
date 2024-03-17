using Kysect.DotnetProjectSystem.SolutionModification;
using System.IO.Abstractions;

namespace Kysect.Zeya.LocalRepositoryAccess;

public class LocalRepository(string repositoryRootPath, string solutionSearchMask, IFileSystem fileSystem, DotnetSolutionModifierFactory solutionModifierFactory) : ILocalRepository
{
    public LocalRepositoryFileSystem FileSystem { get; } = new LocalRepositoryFileSystem(repositoryRootPath, fileSystem);
    public LocalRepositorySolutionManager SolutionManager { get; } = new LocalRepositorySolutionManager(repositoryRootPath, solutionSearchMask, fileSystem, solutionModifierFactory);

    public virtual string GetRepositoryName()
    {
        IDirectoryInfo directoryInfo = fileSystem.DirectoryInfo.New(repositoryRootPath);
        return directoryInfo.Name;
    }
}