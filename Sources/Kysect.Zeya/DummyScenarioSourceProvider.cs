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
               - Name: LicenseFileExists
                 Parameters:
                   Null: Null
               """;
    }
}