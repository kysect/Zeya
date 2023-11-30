using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya;

public class DummyScenarioSourceProvider : IScenarioSourceProvider
{
    public DummyScenarioSourceProvider()
    {
    }

    public IReadOnlyCollection<string> GetScenarioNames()
    {
        return [];
    }

    public string GetScenarioSourceCode(string scenarioName)
    {
        // TODO: remove Null: Null after lib will support steps without arguments
        return """
               - Name: SourceCodeDirectoryExists
                 Parameters:
                   ExpectedSourceDirectoryName: Sources
               - Name: CentralPackageManagerEnabled
                 Parameters:
                   Null: Null
               - Name: ValidateTargetFrameworkVersion
                 Parameters:
                   AllowedVersions: [net8.0, netstandard2.0]
               - Name: ValidateCentralPackageManagerConfig
                 Parameters:
                   MasterFile: MasterDirectory.Packages.props
               - Name: ContainsRequiredFile
                 Parameters:
                   FilePath: LICENSE
                   Sample: Sample\LICENSE
               - Name: ContainsRequiredFile
                 Parameters:
                   FilePath: .github\workflows\build-test.yml
                   Sample: Sample\build-test.yml
               - Name: ContainsRequiredFile
                 Parameters:
                   FilePath: .github\workflows\nuget-publish.yml
                   Sample: Sample\nuget-publish.yml
               - Name: GithubActionWorkflowConfigured
                 Parameters:
                   FileName: dotnet.yml
               - Name: DirectoryBuildPropsContainsRequiredFields
                 Parameters:
                   RequiredFields: ["Authors", "Company", "PackageReadmeFile", "PackageLicenseFile", "Nullable", "LangVersion", "ImplicitUsings", "RepositoryUrl", "Version"]
               """;
    }
}