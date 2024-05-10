namespace Kysect.Zeya.GitIntegration.Abstraction;

public interface IGitIntegrationServiceFactory
{
    IGitIntegrationService CreateGitIntegration(RemoteGitHostCredential credential);
}