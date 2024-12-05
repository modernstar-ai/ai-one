# Checklist
Only check the boxes that apply to your PR changed files. 

You must always review the coding standards checkboxes that apply to your Backend/Frontend changes 

## Backend
### Architecture:
#### Api Layer:
- [ ] All Endpoints must be plural names. i.e. Users not User
- [ ] Endpoints must inherit from CarterModule("/api") and follow the same standard as every other endpoint file.
- [ ] 90% of the time, the only endpoints you require in a class are Get All, Get by Id, Create, UpdateById, DeleteById (CRUD). Do not implement trivial endpoints i.e. UpdateUsername, DeleteByName unless necessary.
- [ ] Endpoints must all return IResults and follow the same MediatR pattern. No business logic should live in the endpoints.
- [ ] Any new configuration variables added in appsettings must be added under **Framework/Agile.Framework.Common/EnvironmentVariables/Configs.cs**
- [ ] Any new constant variables must be added in **Constants.cs** under the same EnvironmentVariables folder listed 1 checkbox above.
- [ ] Any new constant variables must be added in **Constants.cs** under the same EnvironmentVariables folder listed 1 checkbox above.
- [ ] You must **not** reference domain layer aggregates or entities from the Api layer. Only ValueObjects are allowed in Dtos
#### Application Layer:
- [ ] Your business logic for an endpoint must live under a folder with the **exact** same name as the endpoint. Consistency is key!
- [ ] Any Querying of data must live under a **Queries** folder. Any mutations of data must live under a **Commands** folder. To create a command/query, just copy and paste an existing one from another folder and rename accordingly.
- [ ] Object payloads that are sent from the frontend to the Application layer must live under the **Dtos** folder
- [ ] Services that connect to Cosmos Db must live under the **Services** folder and inherit from ICosmosRepository<YOUR_DOMAIN_MODEL>. This gives you basic CRUD operations already i.e. you dont need to implement an AddItem method unless you require additional business logic.
- [ ] Most of your validation logic should live under the **Validator : AbstractValidator<>** class in your commands/queries. This is run automatically before your command/query is handled. Try and minimise validation in the handler method unless necessary
- [ ] Any Scoped, Transient or Singleton service you want to add must be added using the **[Export]** attribute I developed. This is then added automatically on startup
#### Domain Layer:
- [ ] Your domain logic for an endpoint must live under a folder with the **exact** same name as the endpoint. Consistency is key!
- [ ] Inside the folder, you must place Aggregates, Entities and ValueObjects in their appropriate folder. Search up the difference between or ask me if you are unsure how they work in Domain Driven Design.
- [ ] All aggregates must inherit from either AggregateRoot or AuditableAggregateRoot (adds createdAt and lastModified dates to the object too)
- [ ] All aggregates must have private constructors (annotated with a **[JsonConstructor]** attribute) and private setters in properties. They must not be modifiable through the application layer and must only be modifiable within the object itself.
- [ ] All aggregates must have a public static AggregateType Create() method to handle creations of instances and a public static void Update() method to update an aggregate, its entities and value objects.
- [ ] All properties of the aggregate must be defined in the private constructor (to allow for serialization and deserialization of private properties)
#### Framework Layer:
- [ ] The **Agile.Framework.Common** layer must **NOT** have references to outside projects. This allows it to be referencable by any other project and not cause circular dependency errors. Treat it like a nuget package project.
- [ ] All dependency injection of Framework related services for the Api to use must be added under the **Agile.Framework** DependencyInjection.cs file. This is then run in Program.cs
- [ ] Any services that need initialization (i.e. Cosmos to create a database and containers) must have a class that implements the **IAsyncInitializer** interface and exported using the **[Export]** attribute. This is then run automatically on startup of the Api.
### Coding Standards:
- [ ] You must follow most of the C# variable naming standards found [here](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names#naming-conventions) **Very important to stay consistent**
- [ ] I don't want to see a function over 200 lines of code preferably (except for rare cases) to keep code more readable. This also helps immensely with merge conflicts
- [ ] Minimise nesting of conditional logic as much as possible. Invert If statements if you can to help code become more readable and minimal. Humans can only read so deep before they get confused i.e. [Guard Clauses](https://www.linkedin.com/pulse/avoiding-nested-statements-guard-clauses-danny-olsen-a1e8f/)
- [ ] Follow these C# clean code tips where possible [Tips](https://www.google.com/search?q=clean+coding+tips+C%23&sca_esv=e328ce4ff02095a0&sxsrf=ADLYWIIW2E5nT8vmvejkjx-dbYZymqel7g%3A1733401789501&ei=vZxRZ_uyHaH2seMPjOSayQE&ved=0ahUKEwi7hanv0JCKAxUhe2wGHQyyJhkQ4dUDCBA&uact=5&oq=clean+coding+tips+C%23&gs_lp=Egxnd3Mtd2l6LXNlcnAiFGNsZWFuIGNvZGluZyB0aXBzIEMjMgYQABgWGB4yBhAAGBYYHjILEAAYgAQYhgMYigUyCxAAGIAEGIYDGIoFMgsQABiABBiGAxiKBTILEAAYgAQYhgMYigUyBRAAGO8FMgUQABjvBTIFEAAY7wUyCBAAGIAEGKIESOEHUG9YpwRwAXgBkAEAmAHcAaABugSqAQUwLjIuMbgBA8gBAPgBAZgCBKACyATCAgoQABiwAxjWBBhHmAMAiAYBkAYIkgcFMS4yLjGgB4MR&sclient=gws-wiz-serp)
