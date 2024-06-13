# Describe
Back end project using web.api and C# this project is dedicated as template along side front-dancer for FE. For my future projects.
Inspiration src was [this repo](https://github.com/mohamedelareeg/CleanArchitecture/tree/master)


- [Describe](#describe)
- [How to launch project](#how-to-launch-project)
    -[Preconditions for project to launch](#preconditions-for-project-to-launch)
- [Project Architecture](#how-to-apply-migrations)


# How to launch project
This section is dedicated for storing information about how to launch project for development.

## Preconditions for project to launch
WIP


# How to apply migrations
WIP

# Project Architecture
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