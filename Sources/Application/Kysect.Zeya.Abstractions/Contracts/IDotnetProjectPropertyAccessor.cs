namespace Kysect.Zeya.Abstractions.Contracts;

public interface IDotnetProjectPropertyAccessor
{
    bool IsManagePackageVersionsCentrally(string projectPath);
    bool IsTestProject(string projectPath);
    string GetTargetFramework(string projectPath);
}