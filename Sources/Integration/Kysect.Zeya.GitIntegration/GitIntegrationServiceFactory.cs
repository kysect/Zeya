using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Exceptions;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.Zeya.GitIntegration.Abstraction;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.GitIntegration;

public class GitIntegrationServiceFactory(ILogger<IGitIntegrationServiceFactory> logger, GitCommitAuthor? gitCommitAuthor) : IGitIntegrationServiceFactory
{
    public IGitIntegrationService CreateGitIntegration(RemoteGitHostCredential credential)
    {
        credential.ThrowIfNull();

        RepositoryFetchOptions repositoryFetchOptions = CreateRepositoryFetchOptions(credential);
        IRepositoryFetcher repositoryFetcher = new RepositoryFetcher(repositoryFetchOptions, logger);
        repositoryFetcher = new ExceptionHandlerRepositoryFetcherDecorator(repositoryFetcher, logger);

        return new GitIntegrationService(
            gitCommitAuthor,
            repositoryFetcher,
            credential);
    }

    private static RepositoryFetchOptions CreateRepositoryFetchOptions(RemoteGitHostCredential credential)
    {
        return credential.AuthType switch
        {
            GitCredentialType.UserPassword => RepositoryFetchOptions.CreateWithUserPasswordAuth(credential.Username, credential.Token),
            GitCredentialType.HeaderBased => RepositoryFetchOptions.CreateHeaderBasedAuth(credential.Token),
            _ => throw SwitchDefaultExceptions.OnUnexpectedValue(credential.AuthType)
        };
    }
}