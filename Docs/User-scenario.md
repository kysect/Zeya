# User scenarios

## Local repository analysis

User can use Zeya to analyze a local repository and create a pull request with the fix.

```plantuml

actor User as User
participant "Zeya Terminal" as ZeyaTui
participant Github as Github
participant "Repository validator" as RepositoryValidation

User -> ZeyaTui : Analyze repository
ZeyaTui -> Github : Clone repository 
Github -> ZeyaTui : Local repository
ZeyaTui -> Git : Create local branch
ZeyaTui -> RepositoryValidation : Validate repository
ZeyaTui -> RepositoryValidation : Fix repository
ZeyaTui -> Github : Push changes
ZeyaTui -> Github : Create PR

User -> Github : Merge PR
```

## Repository analysis as a service

User can use Zeya Web Service to analyze a repository and create a pull request with the fix.

```plantuml

actor User as User
participant "Zeya Web API" as ZeyaWebApi
participant "Zeya Web Server" as ZeyaWebServer
participant Github as Github
participant "Repository validator" as RepositoryValidation

User -> ZeyaWebApi : Add permission to repository for Zeya
ZeyaWebApi -> User : Redirect to Github
User -> Github : Add permission to repository for Zeya

User -> ZeyaWebApi : Analyze repository
ZeyaWebApi -> ZeyaWebServer : Analyze repository


ZeyaWebServer -> Github : Clone repository 
Github -> ZeyaWebServer : Local repository
ZeyaWebServer -> Git : Create local branch
ZeyaWebServer -> RepositoryValidation : Validate repository
ZeyaWebServer -> RepositoryValidation : Fix repository
ZeyaWebServer -> Github : Push changes
ZeyaWebServer -> Github : Create PR

User -> Github : Merge PR
```