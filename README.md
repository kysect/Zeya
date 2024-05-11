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

## How to build

Step 1. Install dependencies:

```bash
winget install Microsoft.DotNet.AspNetCore.8
# Now this dependency is optional
winget install Docker.DockerDesktop
dotnet workload install aspire
```

Step 2. Create a personal access token for GitHub and Azure DevOps (optional). 
- Add a GitHub token to the user secrets: https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens

Step 3. Add a token to the user secrets:

```bash
dotnet user-secrets init --project Sources/ConfigurationRoot/Kysect.Zeya.WebService
dotnet user-secrets set "RemoteHosts:Github:Token" "ghp_***" --project Sources/ConfigurationRoot/Kysect.Zeya.WebService
dotnet user-secrets set "RemoteHosts:AzureDevOps:Token" "ghp_***" --project Sources/ConfigurationRoot/Kysect.Zeya.WebService
```

Step 4. Build the solution. Solution can be built using the `dotnet` command line tool or Visual Studio.

```bash
dotnet build Sources/Kysect.Zeya.sln
```

## How to run demo

1. Build solution and run Kysect.Zeya.WebAppHost. Aspire will open:
   1. Console
   2. Aspire Dashboard
   3. Zeya Web UI
2. In Web UI go to the `Validation policies` tab and open Demo policy by clicking on '✏️' button.
3. Add GitHub repository to policy by clicking on 'Add Github repository' button. You can add Kysect/Zeya repository.
4. Start validation by clicking on 'Run validation' button.
5. Refresh the page to see the result of the validation.

## How this works

1. Zeya gets a list of repositories from the organization using the GitHub API
2. Repositories are cloned locally using git commands (using LibGit2Sharp)
3. Validation is performed for each repository
   - A yaml file with a validation scenario is loaded and parsed
   - For each rule in the scenario, a check is performed
   - If the check report about some problem, a diagnostic message is created
4. Fixers are applied to the repository if available