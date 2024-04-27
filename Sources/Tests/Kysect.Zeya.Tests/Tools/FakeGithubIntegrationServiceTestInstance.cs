using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.Tests.Tools.Fakes;

namespace Kysect.Zeya.Tests.Tools;

public static class FakeGithubIntegrationServiceTestInstance
{
    public static FakeGithubIntegrationService Create()
    {
        return Create(new FakePathFormatStrategy(string.Empty));
    }

    public static FakeGithubIntegrationService Create(ILocalStoragePathFactory pathFactory)
    {
        return new FakeGithubIntegrationService(
            new GithubIntegrationCredential(),
            pathFactory,
            TestLoggerProvider.GetLogger());
    }
}