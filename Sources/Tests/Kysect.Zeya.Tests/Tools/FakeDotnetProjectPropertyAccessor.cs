using Kysect.Zeya.ManagedDotnetCli;

namespace Kysect.Zeya.Tests.Tools;

// TODO: rework design
public class FakeDotnetProjectPropertyAccessor : IDotnetProjectPropertyAccessor
{
    public string TargetFramework { get; set; }

    public bool IsManagePackageVersionsCentrally(string projectPath)
    {
        throw new NotImplementedException();
    }

    public bool IsTestProject(string projectPath)
    {
        throw new NotImplementedException();
    }

    public string GetTargetFramework(string projectPath)
    {
        return TargetFramework;
    }
}