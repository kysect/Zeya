# Kysect.Zeya

[![codecov](https://codecov.io/github/kysect/Zeya/graph/badge.svg?token=3UGRWLL7JF)](https://codecov.io/github/kysect/Zeya)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fkysect%2FZeya.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fkysect%2FZeya?ref=badge_shield)

> Это Proof of Concept. Поведение и API могут меняться.

Zeya - приложение для мониторинга за GitHub репозиториями, которые содержат исходный код NuGet пакетов. Zeya позволяет валидировать репозитории по заранее описанным правилам и находить проблемы в репозиториях. Это позволяет проверить Healthcheck репозиториев.

Реализованные валидации:

- Валидация репозитория
  - Наличие LICENSE файла
  - Наличие Readme файла
  - Настройки branch protection - обязательные ревью для Pull request'ов
  - Настройки автоматического удаления репозиториев после мёрджа Pull Request'ов
  - Наличие GitHub workflow для сборки проекта
- Валидация исходного кода:
  - Использование только разрешённых версий dotnet (например, только net8.0 и netstandard2.0)
  - Использование Central Package Management
  - Валидация версий Central Package Management для синхронизации между репозиториями
  - Валидация наличия обязательных подключённых nuget пакетов (например, обязательных анализаторов)
  - Использование Artifact Output
- Валидация конфигурации метаданных для нюгета:
  - Наличие указания автора, лицензии, ссылки на репозиторий
  - Использование Symbol Packages


[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fkysect%2FZeya.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Fkysect%2FZeya?ref=badge_large)

## Rules

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
| **Fixer available** | Not implemented                                          |
| **Fixer behavior**  | Replace local CPM versions with versions from master CPM |

| Property            | Value                                            |
|---------------------|--------------------------------------------------|
| **Rule ID**         | SRC00013                                         |
| **Title**           | Required packages added to solution              |
| **Fixer available** | Not implemented                                  |
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

## Алгоритм работы

В рамках Proof of Concept был реализован демо сценарий в котором выполняется такой набор действий:

- Получается список репозиториев из организации по средствам GitHub API
- Клонируются локально репозитории используя git команды (используя LibGit2Sharp)
- По каждому репозиторий запускается валидация
  - Загружается yaml файл с описанием сценария валидации и парсится
  - По каждому правилу в сценарии выполняется проверка
  - Если проверка находит несоответствие, то создаётся сообщение о несоответствии
- После выполнения валидации всех репозиториев композируются все сообщения
- Сообщения с проблемами выводятся в консоль


## Samples

Пример файла с описанием сценария валидации:

```yaml
- Name: Github.RepositoryLicense
  Parameters:
    OwnerName: Kysect
    Year: 2023
    LicenseType: MIT
- Name: Github.ReadmeExists
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
               
- Name: SourceCode.SourcesMustNotBeInRoot
  Parameters:
    ExpectedSourceDirectoryName: Sources
- Name: SourceCode.TargetFrameworkVersionAllowed
  Parameters:
    AllowedVersions: [net8.0, netstandard2.0]
- Name: SourceCode.CentralPackageManagerEnabled
- Name: SourceCode.CentralPackageManagerVersionSynchronized
  Parameters:
    MasterFile: Samples\MasterDirectory.Packages.props
- Name: SourceCode.RequiredPackagesAdded
  Parameters:
    Packages: ["Kysect.Editorconfig"]
- Name: SourceCode.ArtifactsOutputEnabled
- Name: DirectoryBuildPropsContainsRequiredFields
  Parameters:
    RequiredFields: ["Nullable", "LangVersion", "ImplicitUsings"]

- Name: Nuget.MetadataSpecified
  Parameters:
    RequiredValues: ["PackageIcon", "PackageReadmeFile", "RepositoryUrl", "PublishRepositoryUrl"]
    RequiredKeyValues:
      Authors: Kysect
      Company: Kysect
      PackageLicenseExpression: MIT
      DebugType: portable
      IncludeSymbols: true
      SymbolPackageFormat: snupkg
```

Пример результата работы приложения:

```
19:53:14 warn: [0] Kysect/CommonLib: [GHR0002] ReadmeExists.md file was not found
19:53:14 warn: [0] Kysect/CommonLib: [GHR0003] Branch deletion on merge must be enabled.
19:53:14 warn: [0] Kysect/CommonLib: [GHR0004] Workflow build-test.yaml must be configured
19:53:14 warn: [0] Kysect/CommonLib: [GHR0004] Workflow nuget-publish.yaml must be configured
19:53:14 warn: [0] Kysect/CommonLib: [SRC00001] Directory for sources was not found in repository
19:53:14 warn: [0] Kysect/CommonLib: [SRC0005] Package Kysect.Editorconfig is not add to all solution.
19:53:14 warn: [0] Kysect/CommonLib: [SRC0006] Directory.Build.props does not contains UseArtifactsOutput option.
19:53:14 warn: [0] Kysect/CommonLib: [SRC00007] Directory.Build.props field Nullable is missed.
19:53:14 warn: [0] Kysect/CommonLib: [SRC00007] Directory.Build.props field LangVersion is missed.
19:53:14 warn: [0] Kysect/CommonLib: [SRC00007] Directory.Build.props field ImplicitUsings is missed.
19:53:14 warn: [0] Kysect/CommonLib: [NUP0001] Directory.Build.props file does not contains required option: PackageIcon
19:53:14 warn: [0] Kysect/CommonLib: [NUP0001] Directory.Build.props file does not contains required option: PackageReadmeFile
19:53:14 warn: [0] Kysect/CommonLib: [NUP0001] Directory.Build.props file does not contains required option: RepositoryUrl
19:53:14 warn: [0] Kysect/CommonLib: [NUP0001] Directory.Build.props file does not contains required option: PublishRepositoryUrl
19:53:14 warn: [0] Kysect/CommonLib: [NUP0001] Directory.Build.props file does not contains required option: Authors
19:53:14 warn: [0] Kysect/CommonLib: [NUP0001] Directory.Build.props file does not contains required option: Company
19:53:14 warn: [0] Kysect/CommonLib: [NUP0001] Directory.Build.props file does not contains required option: PackageLicenseExpression
19:53:14 warn: [0] Kysect/CommonLib: [NUP0001] Directory.Build.props file does not contains required option: DebugType
19:53:14 warn: [0] Kysect/CommonLib: [NUP0001] Directory.Build.props file does not contains required option: IncludeSymbols
19:53:14 warn: [0] Kysect/CommonLib: [NUP0001] Directory.Build.props file does not contains required option: SymbolPackageFormat
```

## Configutaion

Конфигурация выполняется через appsettings.json и позволяет указать:

- GitHub token
- Организацию, откуда нужно брать репозитории для анализа
- Список репозиториев, которые нужно исключить из анализа


## Future plan

- CLI для запуска валидации
- API для исправления найденных проблем (например, автоматическое добавление полей в Directoty.Build.prop, перевод на Central Package Management)
- Performance (оптимизации и многопоточность)