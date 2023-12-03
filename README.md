# Kysect.Zeya

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
