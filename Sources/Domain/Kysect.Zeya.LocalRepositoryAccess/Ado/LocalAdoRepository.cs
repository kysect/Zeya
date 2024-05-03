using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.Zeya.AdoIntegration.Abstraction;
using System.IO.Abstractions;

namespace Kysect.Zeya.LocalRepositoryAccess.Ado;

public class LocalAdoRepository : ILocalRepository
{
    private readonly string _repositoryRootPath;
    private readonly string _remoteHttpsUrl;
    private readonly IFileSystem _fileSystem;

    public LocalAdoRepository(string repositoryRootPath,
        string remoteHttpsUrl,
        string solutionSearchMask,
        IAdoIntegrationService adoIntegrationServiceIntegrationService,
        IFileSystem fileSystem,
        DotnetSolutionModifierFactory solutionModifierFactory)
    {
        _repositoryRootPath = repositoryRootPath;
        _remoteHttpsUrl = remoteHttpsUrl;
        _fileSystem = fileSystem;
        SolutionManager = new LocalRepositorySolutionManager(repositoryRootPath, solutionSearchMask, fileSystem, solutionModifierFactory);
        AdoIntegrationService = adoIntegrationServiceIntegrationService;

        RepositoryUrlParts = AdoRepositoryUrl.Parse(_remoteHttpsUrl);
    }

    public LocalRepositoryFileSystem FileSystem => new LocalRepositoryFileSystem(_repositoryRootPath, _fileSystem);
    public LocalRepositorySolutionManager SolutionManager { get; }
    public IAdoIntegrationService AdoIntegrationService { get; }

    public AdoRepositoryUrl RepositoryUrlParts { get; }

    public string GetRepositoryName()
    {
        return RepositoryUrlParts.Repository;
    }
}