# Describe

Animal Shelter project's BE part.
FE part of the project [could be found here](https://github.com/DainVerd/animal-shelter-fe)

- [Describe](#describe)
- [How to launch project](#how-to-launch-project)
- [Project Architecture](#project-architecture)
- [Tech stack](#tech-stack)

## How to launch project

This section is dedicated for storing information about how to launch project for development.

0. Download docker desktop. You could [get it from here](https://www.docker.com/products/docker-desktop/)
1. launch docker compose file of the project to install postgre sql container to the docker desktop.
2. clone the repository
3. open project in visual studio
4. Select Web.api as start project
5. Apply DB migrations by exe command in package manager console

    ```bash
    dotnet ef database update --project Infrastructure --startup-project Backend
    ```

6. launch project by pressing `Ctrl + F5`

## Project Architecture

Inspiration src was [this repo](https://github.com/mohamedelareeg/CleanArchitecture/tree/master)

1. CleanArchitecture.Domain
The heart of the application, CleanArchitecture.Domain, holds the domain entities and business logic. It represents the core of your application and remains independent of any external frameworks.

2. CleanArchitecture.Application
CleanArchitecture.Application encapsulates application-specific business rules, use cases, and application services. It acts as a mediator between the domain layer and the infrastructure layer.

3. CleanArchitecture.Infrastructure
CleanArchitecture.Infrastructure contains implementation details that are external to the application. It includes data access, external services, and other infrastructure concerns.

4. CleanArchitecture.Identity
CleanArchitecture.Identity focuses on user identity and authentication aspects, handling user-related functionalities.

5. CleanArchitecture.Persistence
The CleanArchitecture.Persistence project deals with data storage and retrieval, using technologies such as Entity Framework Core to interact with the database.

6. CleanArchitecture.Api
CleanArchitecture.Api serves as the entry point for the Web API application. It utilizes the Clean Architecture principles to handle incoming HTTP requests and coordinate actions across different layers.

## Tech stack

- ASP.NET(web.api)
- C#
- EF Core
- PostgreSQL as docker container

Unit tests:

- awesomeAssertions
- xUnit
- Moq
