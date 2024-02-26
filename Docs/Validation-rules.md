
# Rules

List of available rules and their properties.

| Property            | Value                                |
|---------------------|--------------------------------------|
| **Rule ID**         | SRC00001                             |
| **Title**           | Do not store code in repository root |
| **Fixer available** | No                                   |

| Property            | Value                                                                            |
|---------------------|----------------------------------------------------------------------------------|
| **Rule ID**         | SRC00002                                                                         |
| **Title**           | Use actual dotnet version                                                        |
| **Fixer available** | Not implemented                                                                  |
| **Fixer behavior**  | Update only .NET Core versions to specified. Do not affect Framework or Standard |

| Property            | Value                                                    |
|---------------------|----------------------------------------------------------|
| **Rule ID**         | SRC00003                                                 |
| **Title**           | Use artifacts output                                     |
| **Fixer available** | Yes                                                      |
| **Fixer behavior**  | Add UseArtifactsOutput property to Directory.Build.props |

| Property            | Value                                                                  |
|---------------------|------------------------------------------------------------------------|
| **Rule ID**         | SRC00010                                                               |
| **Title**           | Use Nuget Central Package Management                                   |
| **Fixer available** | Yes                                                                    |
| **Fixer behavior**  | Create Directory.Packages.props files and remove versions from .csproj |

| Property            | Value                                          |
|---------------------|------------------------------------------------|
| **Rule ID**         | SRC00011                                       |
| **Title**           | Nuget versions must be specified in master CPM |
| **Fixer available** | No                                             |

| Property            | Value                                                    |
|---------------------|----------------------------------------------------------|
| **Rule ID**         | SRC00012                                                 |
| **Title**           | Nuget version must be synchronized with master CPM       |
| **Fixer available** | Yes                                                      |
| **Fixer behavior**  | Replace local CPM versions with versions from master CPM |

| Property            | Value                                            |
|---------------------|--------------------------------------------------|
| **Rule ID**         | SRC00013                                         |
| **Title**           | Required packages added to solution              |
| **Fixer available** | Yes                                              |
| **Fixer behavior**  | Add Nuget package using to Directory.Build.props |

| Property            | Value                                 |
|---------------------|---------------------------------------|
| **Rule ID**         | GHR0001                               |
| **Title**           | Repository must contains license file |
| **Fixer available** | Not implemented                       |
| **Fixer behavior**  | Add license file to repository        |

| Property            | Value                                |
|---------------------|--------------------------------------|
| **Rule ID**         | GHR0002                              |
| **Title**           | Repository must contains readme file |
| **Fixer available** | No                                   |

| Property            | Value                                              |
|---------------------|----------------------------------------------------|
| **Rule ID**         | GHR0003                                            |
| **Title**           | Branch protection must be enabled on master branch |
| **Fixer available** | Not implemented                                    |
| **Fixer behavior**  | Set default policy for branch protection           |

| Property            | Value                                |
|---------------------|--------------------------------------|
| **Rule ID**         | GHR0004                              |
| **Title**           | Branch auto deletion must be enabled |
| **Fixer available** | Not implemented                      |
| **Fixer behavior**  | Enable branch deletion after PR      |

| Property            | Value                                         |
|---------------------|-----------------------------------------------|
| **Rule ID**         | GHR0005                                       |
| **Title**           | Github Action should be configured            |
| **Fixer available** | Not implemented                               |
| **Fixer behavior**  | Add Github Action workflow file to repository |

| Property            | Value                                     |
|---------------------|-------------------------------------------|
| **Rule ID**         | NUP0001                                   |
| **Title**           | Required Nuget metadata must be specified |
| **Fixer available** | No                                        |

| Property            | Value                                 |
|---------------------|---------------------------------------|
| **Rule ID**         | NUP0002                               |
| **Title**           | Nuget metadata values must be correct |
| **Fixer available** | Yes                                   |
| **Fixer behavior**  | Add or update Nuget metadata values   |
