using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.Zeya.AdoIntegration.Abstraction;
using System.IO.Abstractions;

namespace Kysect.Zeya.LocalRepositoryAccess.Ado;

public class LocalAdoRepository(
    string repositoryRootPath,
    string remoteHttpsUrl,
    string solutionSearchMask,
    IAdoIntegrationService adoIntegrationServiceIntegrationService,
    IFileSystem fileSystem,
    DotnetSolutionModifierFactory solutionModifierFactory)
    : ILocalRepository
{
    public LocalRepositoryFileSystem FileSystem => new LocalRepositoryFileSystem(repositoryRootPath, fileSystem);
    public LocalRepositorySolutionManager SolutionManager { get; } = new LocalRepositorySolutionManager(repositoryRootPath, solutionSearchMask, fileSystem, solutionModifierFactory);
    public IAdoIntegrationService AdoIntegrationService { get; } = adoIntegrationServiceIntegrationService;

    public string GetRepositoryName()
    {
        // TODO: rework this
        return remoteHttpsUrl.Split("/").Last();
    }
}