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
#### Application Layer:
- [ ] Your business logic for an endpoint must live under a folder with the **exact** same name as the endpoint. Consistency is key!
- [ ] Any Querying of data must live under a **Queries** folder. Any mutations of data must live under a **Commands** folder. To create a command/query, just copy and paste an existing one from another folder and rename accordingly.
- [ ] Object payloads that are sent from the frontend to the Application layer must live under the **Dtos** folder
- [ ] Services that connect to Cosmos Db must live under the **Services** folder and inherit from ICosmosRepository<YOUR_DOMAIN_MODEL>. This gives you basic CRUD operations already i.e. you dont need to implement an AddItem method unless you require additional business logic.
- [ ] Most of your validation logic should live under the **Validator : AbstractValidator<>** class in your commands/queries. This is run automatically before your command/query is handled. Try and minimise validation in the handler method unless necessary

### Coding Standards:
