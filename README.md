# Kysect.Zeya

[![codecov](https://codecov.io/github/kysect/Zeya/graph/badge.svg?token=3UGRWLL7JF)](https://codecov.io/github/kysect/Zeya)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fkysect%2FZeya.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fkysect%2FZeya?ref=badge_shield)

> Proof of Concept. API may be changed.

Zeya is a tool for monitoring and validating GitHub repositories that contain the source code of .NET solutions. Zeya allows you to validate repositories according to pre-defined rules and find problems in repositories.

Samples of rules:
- Repository must contains license file
- Repository has correct branch protection configuration
- Repository contains GitHub workflow for building project
- .NET solution use Central Package Management
- .NET solution use artifact ouput layout
- .NET solution use required Nuget packages (like analyzers or code style packages)
- .NET solution use symbols packages for Nuget packages

All rules documented in the [Docs/Validation-rules.md](Docs/Validation-rules.md).

## How to use

1. Create a yaml file with a validation scenario
2. Run the Zeya application with the path to the yaml file as an argument and Github repository information
3. Get the result of the validation
4. Fix the problems found in the repositories
5. Run the Zeya application again to check the result

Yaml file example:

```yaml
- Name: Github.RepositoryLicense
  Parameters:
    OwnerName: Kysect
    Year: 2024
    LicenseType: MIT

- Name: Github.BranchProtectionEnabled
  Parameters:
    PullRequestReviewRequired: true
    ConversationResolutionRequired: true
- Name: Github.AutoBranchDeletionEnabled

- Name: Github.BuildWorkflowEnabled
  Parameters:
    MasterFile: Samples\build-test.yaml
- Name: Github.BuildWorkflowEnabled
  Parameters:
    MasterFile: Samples\nuget-publish.yaml

- Name: SourceCode.CentralPackageManagerEnabled
- Name: SourceCode.ArtifactsOutputEnabled
- Name: SourceCode.RequiredPackagesAdded
  Parameters:
    Packages: ["Kysect.Editorconfig"]

- Name: Nuget.MetadataSpecified
  Parameters:
    RequiredKeyValues:
      DebugType: portable
      IncludeSymbols: true
      SymbolPackageFormat: snupkg
```

## How this works

1. Zeya gets a list of repositories from the organization using the GitHub API
2. Repositories are cloned locally using git commands (using LibGit2Sharp)
3. Validation is performed for each repository
   - A yaml file with a validation scenario is loaded and parsed
   - For each rule in the scenario, a check is performed
   - If the check report about some problem, a diagnostic message is created
4. Fixers are applied to the repository if available

## How to build

Install dependencies:

```bash
winget install Microsoft.DotNet.AspNetCore.8
winget install Docker.DockerDesktop
winget install GitHub.cli
dotnet workload install aspire
```

Configure settings:
- Add a GitHub token to the user secrets: https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens
- 

```bash

Solution can be built using the `dotnet` command line tool or Visual Studio.

```bash
dotnet build Sources/Kysect.Zeya.sln
```

## Fixers

Fixers are a set of rules that can be applied to the repository to fix problems. Some samples of fixers:

- Automatically update .NET version
- Add or update Nuget metadata values to Directory.Build.props
- Add UseArtifactsOutput property to Directory.Build.props
- Create Directory.Packages.props files and remove versions from .csproj
- Add Nuget package using to Directory.Build.props