using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.Abstractions.Contracts;

public interface IGithubIntegrationService
{
    void CloneOrUpdate(GithubRepository repository);
}