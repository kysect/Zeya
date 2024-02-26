# Solution architecture

## Context diagram

```plantuml
@startuml C4 Context diagram
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

Person(webUser, "User", "A person who uses the Zeya web service")
System(zeyaService, "Zeya Web Service")
System_Ext(github, "GitHub")

Rel(webUser, zeyaService, "Uses")
Rel(zeyaService, github, "Loads repository information and push changes")

@enduml
```

## Container diagram

```plantuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

Person(webUser, "User", "A person who uses the Zeya web service")
System_Ext(github, "GitHub")

System_Boundary(container, "Container") {
    Container(frontend, "Frontend")
    Container(backend, "Backend")
    ContainerDb(db, "Database", "PostgreSQL")

    Rel(frontend, backend, "Uses")
    Rel(backend, db, "Uses")
    Rel(backend, github, "Uses")
}

Rel(webUser, frontend, "Request analysis for repository")
Rel(webUser, github, "Approve Pull Request")
```

## Component diagram

```plantuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

Person(webUser, "User", "A person who uses the Zeya web service")
System_Ext(github, "GitHub")
ContainerDb(db, "PostgreSQL")

System_Boundary(container, "Container") {
    Container(frontend, "Kysect.Zeya.WebClient")
    Container(api, "Kysect.Zeya.WebService")
    Container(application, "Kysect.Zeya.IntegrationManager")
    Container(validation, "Kysect.Zeya.RepositoryValidation")

    Container(dataAccess, "Kysect.Zeya.DataAccess.EntityFramework")
    Container(githubIntegration, "Kysect.Zeya.GithubIntegration")
    Container(gitIntegration, "Kysect.Zeya.GitIntegration")

    Container_Ext(fileSystem, "File system")
    Container_Ext(localGit, "Git")
    Container_Ext(githubCli, "Github CLI")

    Rel(frontend, api, "Uses")
    Rel(api, application, "Uses")
    Rel(application, validation, "Uses")
    Rel(validation, fileSystem, "Uses")

    Rel(application, dataAccess, "Uses")
    Rel(dataAccess, db, "Uses")

    Rel(application, githubIntegration, "Uses")
    Rel(githubIntegration, githubCli, "Uses")
    Rel(githubIntegration, github, "Uses")
    Rel(githubIntegration, fileSystem, "Uses")

    Rel(application, gitIntegration, "Uses")
    Rel(gitIntegration, localGit, "Uses")
    Rel(gitIntegration, fileSystem, "Uses")
}

```