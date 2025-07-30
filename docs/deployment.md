# AI-One Deployment Guide

The guide provides step-by-step instructions for deploying the AI-One application on Azure.

The deployment process provisions the following solution components:

- **Platform**: Core infrastructure and shared services that provide foundational capabilities for the solution.
- **Web App**: Frontend application to interact with the application.
- **Backend services**: Handles business logic, data processing, indexing and integration with other services.

---

## 1. Infrastructure Overview

The infrastructure resources are defined using modular Bicep templates. All templates and scripts are located in the `infra` directory of the project repository.

- `platform/core`: shared infrastructure resources.
- `platform/ai`: cognitive services, and AI related resources.
- `platform/monitoring`: logging, monitoring, and alerting resources.
- `networking/`: virtual networks, subnets, and related networking resources.
- `backend/`: backend application resources.
- `frontend/`: frontend application resources.

```plaintext

infra/
│
├── platform/
│   ├── monitoring/
│   │   └── main.bicep
│   ├── core/
│   │   └── main.bicep
│   ├── ai/
│      └── main.bicep
│
├── networking/
│   └── main.bicep
│
├── backend/
│   └── main.bicep
│
├── frontend/
    └── main.bicep

```

### 1.1 Platform

The Platform resources are divided into three modules:

#### 1.1.1 Monitoring

The monitoring infrastructure resources are defined in the `infra/platform/monitoring/main.bicep` file.

| Resource                  | Purpose                                  |
|---------------------------|------------------------------------------|
| Log Analytics Workspace   | Centralized logging and monitoring       |

#### 1.1.2 Core infrastructure

The Core infrastructure resources are defined in the `infra/platform/core/main.bicep` file. This provisions all the shared resources required for the AI-One solution.

| Resource           | Purpose                                                                 |
|--------------------|-------------------------------------------------------------------------|
| Key Vault          | Secure storage for secrets and keys                                      |
| Storage Account    | General-purpose storage                                                  |
| App Service Plan   | Host web application and backend services                                          |
| Service Bus        | Handles asynchronous messaging between solution components for event-driven workflows |
| Azure AI Search    | Enables intelligent search functionality in the application              |
| Cosmos DB Account  | Stores application data                                                  |

#### 1.1.3 AI Services

The `infra/platform/ai/main.bicep` file provisions cognitive services and Large Language Models (LLMs).

| Resource                | Purpose                                  |
|-------------------------|------------------------------------------|
| AI Foundry             | AI reource management and deployment |
| Document Intelligence   | AI-powered document analysis             |
| Azure OpenAI Service    | Access to Azure OpenAI models            |

### 1.2 Networking

AI-One solution can be deployed in a private network. The networking resources are defined in the `infra/networking/main.bicep` file.

| Resource                  | Purpose                                                        |
|---------------------------|----------------------------------------------------------------|
| Virtual Network           | Provides network isolation and connectivity for resources     |
| Subnets                   | Segments the virtual network for different resource types     |
| Network Security Groups   | Controls inbound and outbound traffic to resources            |

### 1.3 Backend

The `infra/backend/apiapp.bicep` file provisions the backend resources and supporting services required for the application.

| Resource                  | Purpose                                                        |
|---------------------------|----------------------------------------------------------------|
| API App                   | Hosts the API application                                      |
| Cosmos DB Database        | Stores application data                                        |
| Blob Containers           | Store files uploaded for indexing                     |
| Event Grid System Topic   | Publishes events to trigger document indexing workflows        |
| Event Grid Subscription   | Subscribes to indexing events and routes them to processing endpoints |
| Application Insights      | Application monitoring and telemetry                           |

### 1.4 Frontend

The `infra/frontend/main.bicep` file provisions the frontend web application resources.

| Resource                  | Purpose                                                        |
|---------------------------|----------------------------------------------------------------|
| Web App                   | Hosts the frontend web application                             |
|Application Insights      | Application monitoring and telemetry                           |

---

## 2. AI-One Deployment

### 2.1 Prerequisites

1. **Azure Subscription**

    An active Azure subscription to deploy resources. For enhanced isolation and security, consider using a separate Azure subscription for each environment (e.g., `dev`, `tst`, `uat`, `prod`).

2. **Developer Access**

    The Development team must have **Contributor** access to non-production environments (e.g., `dev`, `tst`, `uat`) and **Reader** role for production to allow monitoring and troubleshooting without making changes.

3. **Resource Naming Convention**

    AI-One follows a standardized naming convention for all Azure resources. The default format is:

    ```
    <projectName>-<environment>-<resourceType>
    ```

    - `<projectName>`: The project identifier (default: `ag-aione`).
    - `<environment>`: The deployment environment (e.g., `dev`, `tst`, `uat`, `prod`).
    - `<resourceType>`: The short name or abbreviation for the Azure resource (e.g., `vnet`, `cosmos`, `kv`, `sa`).

    **Example:**  
    - `ag-aione-dev-vnet` for the development environment's virtual network  
    - `ag-aione-prod-cosmos` for the production Cosmos DB account

    Confirm the naming convention and the value of `projectName` with the Customer.

4. **Service Principal**

    A service principal is required to securely authenticate and authorize deployment pipelines to access Azure resources. The service principal must be assigned `Contributor` and `User Access Administrator` roles on the Azure subscription where AI-One resources will be deployed.

    - The `Contributor` role allows the pipeline to provision and manage resources.
    - The `User Access Administrator` role is necessary for automated role assignments during deployment.

    You would need a new service principal for each environment (e.g., `dev`, `tst`, `uat`, `prod`) for enhanced security and isolation.

    In the following section, you wil use the service principal to authorize the CI/CD pipeline to deploy resources to Azure.

---

### 2.2 Deployment Steps

### 2.2.1 Application Registration

Create App Registration in Azure Entra ID. The registered application will be used for the below requirements:

- **User and App Authentication**: Authenticate both users and applications with Azure Entra ID.
- **API Access**: Grant the application permission to access Microsoft Graph and other protected APIs.

You would need a new app registration for each environment (e.g., `dev`, `tst`, `uat`, `prod`) to isolate resources and permissions.

1. **Register a New Application**

    1. Sign in to the [Azure Portal](https://portal.azure.com).
    2. In the search bar, enter **App registrations** and select it.
    3. Click **+ New registration**.
    4. Complete the registration form:
        - **Name**: Enter a descriptive name (e.g., `My Company AI Platform - Dev`).
        - **Supported account types**: Select **Single tenant**.
        - **Redirect URI**: Leave blank for now. This will be configured after resource deployment.
    5. Click **Register**.
    6. Note the following details from the overview page:
        - **Application (client) ID**: Unique identifier for your application.
        - **Directory (tenant) ID**: Identifier for your Azure AD tenant.

2. **Assign Owners**
    1. In the app registration, navigate to **Manage > Owners**.
    2. Add required developers (e.g., `Adam.Stephensen@agile-insights.com.au`) as owners. This will allow them to configure settings and manage credentials after deployment.

3. **Expose an API (for Backend Application)**

    This allows backend applications to define permission scopes and enable secure access for client applications.

    1. In the app registration, go to **Expose an API**.
    2. Set the **Application ID URI** to `api://<Application (client) ID>` (e.g., `api://9ezz5cae-8z55-4zzz-9zzz-0zzzzz630zzz`).
        - If not set, click **Set** and enter the value, then click **Save**.
    3. Under **Scopes defined by this API**, click **Add a scope** and enter:
        - **Scope name**: `User.Read`
        - **Who can consent**: Admins and users
        - **Admin/User Consent Display Name/Description**: `User.Read`
        - **State**: Enabled
        - Click **Add scope**.

4. **Provide API access to Client Applications**

    AI-One will use the same client ID for both the API and web app.

    1. To enable a client application to access this API, go to the **Expose an API** section of your app registration.
        - Click **Add a client application**.
        - Click **Add application** button.

### 2.2.2 Setup Source Code Repository

Set up [agile-chat](https://github.com/agile-internal/agile-chat) source code repository for the AI-One application in the Customer's GitHub organization or Azure DevOps project.

The source code repository includes the application code, Bicep templates, CI/CD pipelines and deployment scripts.

1. **Setup Environment Parameters**: The AI-One solution uses parameters to define environment-specific configurations. The parameters are defined in the `infra` directory and are used during deployment to customize the resources for each environment.

    Based on the Customer's requirements of the number of environments, add or remove the parameter files (`[env].bicepparam`) for each environment in the `infra` directory.

    ```plaintext

    infra/
    │
    ├── platform/
    │   ├── monitoring/
    │   │   └── main.bicep
    │   │   └── [env].bicepparam
    │   ├── core/
    │   │   └── main.bicep
    │   │   └── [env].bicepparam
    │   ├── ai/
    │   │   └── main.bicep
    │   │   └── [env].bicepparam
    │
    ├── networking/
    │   └── main.bicep
    │   └── [env].bicepparam
    │
    ├── backend/
    │   └── main.bicep
    │   └── [env].bicepparam
    │
    ├── frontend/
    │   └── main.bicep
    │   └── [env].bicepparam

    ```

2. **Define Resource Tags**: Update the file `infra/tags.json` to define the resource tags for the AI-One solution. The tags will be applied to all resources provisioned by the Bicep templates.

### 2.2.3 Configure CI/CD Pipelines

AI-One deployment supports both Azure DevOps and GitHub Actions.
Based on the Customers' preference, follow the steps below to configure the CI/CD pipelines.

1. **Configure Permissions**: To enable your CI/CD pipeline to deploy resources to Azure, you must grant necessary permissions using the service principal created earlier.

    Based on the Customers' preference, follow the steps below to configure the CI/CD pipelines:

    ***GitHub***

     - [Configure Permissions for GitHub Actions](#312-configure-permissions-for-github-actions).

    ***Azure DevOps***

    - [Configure Permissions for Azure DevOps Pipelines](#321-configure-permissions-for-azure-devops-pipelines).

2. **Environment Configuration**: Create the following environment variables in the CI/CD platform.

    - `AZURE_TENANT_ID`: The Azure Active Directory tenant ID.
    - `AZURE_SUBSCRIPTION_ID`: The Azure subscription ID.
    - `AZURE_CLIENT_ID`: The client ID of the Azure service principal used for deployment.
    - `AZURE_LOCATION`: The Azure region where resources will be deployed (e.g., `australiaeast`).
    - `AZURE_OPENAI_LOCATION`: The Azure region for Azure OpenAI resources (e.g., `australiaeast`).
    - `AZURE_RESOURCE_GROUP`: The name of the Azure resource group for the deployment.
    - `PROJECT_NAME`: The project identifier (default: `ag-aione`).
    - `VITE_AGILECHAT_API_URL`: The API endpoint for the backend (e.g., `https://ag-aione-dev-apiapp.azurewebsites.net`).

        The url is in the format `https://<projectName>-<environment>-apiapp.azurewebsites.net`.

    Refer to the steps for details on how to create environments and environment variables.

    ***GitHub***

     - [Create Environments in GitHub](#313-create-environments-in-github)
     - [Create Environment Variables in GitHub](#314-create-environment-variables-in-github)

    ***Azure DevOps***

     - [Create Environments in Azure DevOps](#322-create-environments-in-azure-devops)
     - [Create Environment Variables in Azure DevOps](#323-create-environment-variables-in-azure-devops)

3. **Create CI/CD Pipelines**: Create the following CI/CD pipelines to deploy the AI-One solution.

    - **Deploy Monitoring Infrastructure (Optional)**: Provisions the Log Analytics Workspace and related monitoring resources.
    - **Deploy Network Infrastructure (Optional)**: Provisions virtual networks, subnets, and network security groups for resource isolation and connectivity.
    - **Deploy AI-One Platform Infrastructure**: Provisions core platform resources, including shared services, cognitive services, and AI-related components.
    - **Deploy Backend API**: Deploys the backend API application and its supporting Azure resources.
    - **Deploy Frontend Web App**: Deploys the frontend web application.
    - **Deploy Application Gateway (Optional)** - Deploys the Application Gateway to expose the AI-One application securely over the internet.

    ***GitHub***

     Github will automatically create the CI/CD pipelines when you push the code to the repository. The pipelines will be defined in the `.github/workflows` directory.

    ***Azure DevOps***

    The Azure DevOps pipelines are defined in the `.azure-pipelines` directory. Create the pipelines using [Create Azure DevOps Pipelines](#324-create-azure-devops-pipelines). The path to the YAML files for each pipeline is as follows:

    - **Deploy Monitoring Infrastructure**: `.azure-pipelines/deploy-monitoring.yml`
    - **Deploy Network Infrastructure**: `.azure-pipelines/deploy-network.yml`
    - **Deploy AI-One Platform Infrastructure**: `.azure-pipelines/deploy-platform.yml`
    - **Deploy Backend API**: `.azure-pipelines/deploy-backend.yml`
    - **Deploy Frontend Web App**: `.azure-pipelines/deploy-frontend.yml`
    - **Deploy Application Gateway**: `.azure-pipelines/deploy-appgw.yml`

### 2.2.4 Deploy Log Analytics Workspace

The Log Analytics Workspace is used for centralized logging and monitoring of the AI-One solution.
If the Customer already has a Log Analytics Workspace, you can use the existing workspace and skip this step.

```plaintext

    infra/
    │
    ├── platform/
    │   ├── monitoring/
    │   │   └── main.bicep
    │   │   └── [env].bicepparam

```

Deployment Pipeline: `Deploy Monitoring Infrastructure`

Bicep File: `infra/platform/monitoring/main.bicep`

Bicep Parameters: `infra/platform/monitoring/[env].bicepparam`

| Parameter Name    | Description                                                                 | Default Value    |
|-------------------|-----------------------------------------------------------------------------|------------------|
| `environmentName` | Name of the deployment environment (e.g., `dev`, `tst`, `uat`, `prod`).     |         |
| `projectName`     | Project identifier used for resource naming.                                | `ag-aione`       |
| `location`        | Azure region where resources will be deployed.                              | `australiaeast`  |
| `tags`            | Key-value pairs for resource tags (from `infra/tags.json`).                 |         |

### 2.2.4 Deploy Networking Infrastructure

The AI-One solution supports deployment in both public and private network configurations. If the application requires network isolation, you can deploy the networking resources required for the solution.

Update `logAnalyticsWorkspaceResourceId` parameter value to the resource ID of the Log Analytics Workspace created in the previous step. If using an existing Log Analytics Workspace, provide its resource ID.

```plaintext

infra/
│
├── networking/
│   └── main.bicep
│   └── [env].bicepparam

```

Deployment Pipeline: `Deploy Networking Infrastructure`

Bicep File: `infra/networking/main.bicep`

Bicep Parameters: `infra/networking/[env].bicepparam`

| Parameter Name                  | Description                                                                                 | Default Value      |
|---------------------------------|---------------------------------------------------------------------------------------------|--------------------|
| `environmentName`               | Name of the deployment environment (e.g., `dev`, `tst`, `uat`, `prod`).                    |                    |
| `projectName`                   | Project identifier used for resource naming.                                                | `ag-aione`         |
| `location`                      | Azure region where resources will be deployed.                                              | `australiaeast`    |
| `tags`                          | Key-value pairs for resource tags (from `infra/tags.json`).                                |                    |
| `vnetConfig`                    | Configuration for the virtual network, including subnets and address prefixes.              | *(see below)*      |
| `nsgConfig`                     | Network Security Groups (NSGs) configuration.                                               | *(see below)*      |
| `logAnalyticsWorkspaceResourceId` | Resource ID of the Log Analytics Workspace.                                                |                    |

**Details for `vnetConfig`:**

- `name`: Name of the virtual network.
- `addressPrefixes`: Address space for the virtual network.
- `vmSubnet`: Virtual Machine subnet.
- `keyVaultSubnet`: Key Vault subnet.
- `storageSubnet`: Storage subnet.
- `cosmosDbSubnet`: Cosmos DB subnet.
- `aiSearchSubnet`: AI Search subnet.
- `serviceBusSubnet`: Service Bus subnet.
- `cognitiveServiceSubnet`: Cognitive Service subnet.
- `appServiceSubnet`: App Service subnet.

**Details for `nsgConfig`:**

- `vmNsgName`: Name of the NSG for Virtual Machines.
- `keyVaultNsgName`: Name of the NSG for Key Vault.
- `storageNsgName`: Name of the NSG for Storage.
- `cosmosDbNsgName`: Name of the NSG for Cosmos DB.
- `aiSearchNsgName`: Name of the NSG for AI Search.
- `serviceBusNsgName`: Name of the NSG for Service Bus.
- `cognitiveServiceNsgName`: Name of the NSG for Cognitive Services.
- `appServiceNsgName`: Name of the NSG for App Services.

### 2.2.4 Deploy Platform Infrastructure

Use the `Deploy Platform Infrastructure` deployment pipeline to provision the platform resources. The pipeline is a multistage pipeline that deploys the core infrastructure and AI services.

Update `logAnalyticsWorkspaceResourceId` parameter value to the resource ID of the Log Analytics Workspace created in the previous step. If using an existing Log Analytics Workspace, provide its resource ID.

The model deployments are defined in the `infra/openai-models.json` file. By default, the following models are deployed:

- `gpt-4o`
- `text-embedding-3-small`

#### Core Infrastructure

```plaintext

    infra/
    │
    ├── platform/
    │   ├── core/
    │   │   └── main.bicep
    │   │   └── [env].bicepparam

```

Bicep File: `infra/platform/core/main.bicep`

Bicep Parameters: `infra/platform/core/[env].bicepparam`

| Parameter Name                   | Description                                                              | Default Value     |
|----------------------------------|--------------------------------------------------------------------------|-------------------|
| `environmentName`                | Name of the deployment environment (e.g., `dev`, `tst`, `uat`, `prod`).  |                   |
| `projectName`                    | Project identifier used for resource naming.                             | `ag-aione`        |
| `location`                       | Azure region where resources will be deployed.                           | `australiaeast`   |
| `tags`                           | Key-value pairs for resource tags (from `infra/tags.json`).              |                   |
| `logAnalyticsWorkspaceResourceId`| Resource ID of the Log Analytics Workspace.              |                   |
| `azureClientId`                  | Azure Client ID, read from environment variables.              |                   |
| `azureTenantId`                  | Azure Tenant ID, read from environment variables.              |                 |
| `networkIsolation`               | Boolean flag to enable or disable network isolation.                     | `false`           |

#### AI Services

```plaintext

    infra/
    │
    ├── platform/
    │   ├── ai/
    │   │   └── main.bicep
    │   │   └── [env].bicepparam

```

Bicep File: `infra/platform/ai/main.bicep`

Bicep Parameters: `infra/platform/ai/[env].bicepparam`

| Parameter Name                    | Description                                                                 | Default Value     |
|-----------------------------------|-----------------------------------------------------------------------------|-------------------|
| `environmentName`                 | Name of the deployment environment (e.g., `dev`, `tst`, `uat`, `prod`).     |                   |
| `projectName`                     | Project identifier used for resource naming.                                | `ag-aione`        |
| `location`                        | Azure region where resources will be deployed.                              | `australiaeast`   |
| `tags`                            | Key-value pairs for resource tags (from `infra/tags.json`).                 |                   |
| `openAILocation`                  | Azure OpenAI location, read from environment variables.                     | `australiaeast`   |
| `logAnalyticsWorkspaceResourceId` | Resource ID of the Log Analytics Workspace.                                 |                   |
| `deployAIFoundryResources`        | Flag to deploy AI Foundry resources.                                        | `true`            |
| `deployOpenAiModels`              | Flag to deploy OpenAI models.                                               | `true`            |
| `networkIsolation`                | Boolean flag to enable or disable network isolation.                        | `false`           |

### 2.2.5 Deploy Backend Application

The pipeline will create the Azure resources and also deploy the backend API application to Azure App Service.

The Backend application would use the platform resources provisioned in the previous step.

Update the parameters file to match the environment configuration of the platform resources.

Update the `logAnalyticsWorkspaceResourceId` parameter value to the resource ID of the Log Analytics Workspace created in the previous step. If using an existing Log Analytics Workspace, provide its resource ID.

> **Important:**  
>
> Deploying the backend application requires permission to create role assignments for its managed identity. If the service principal used for deployment does **not** have sufficient permissions, set the `deployRoleAssignments` parameter to `false` in your deployment configuration.  
>
> When role assignment creation is skipped, you must assign the required roles to the backend application's managed identity after deployment. Refer to the [Assign Roles to Backend API Managed Identity](#41-assign-roles-to-backend-api-managed-identity) section for details.

```plaintext

    infra/
    │
    ├── backend/
    │   └── main.bicep
    │   └── [env].bicepparam

```

Deployment Pipeline: `Deploy Backend API`

Bicep File: `infra/backend/apiapp.bicep`

Bicep Parameters: `infra/backend/[env].bicepparam`

| Parameter Name                    | Description                                                                 | Default Value     |
|-----------------------------------|-----------------------------------------------------------------------------|-------------------|
| `environmentName`                 | Name of the deployment environment (e.g., `dev`, `tst`, `uat`, `prod`).     |                   |
| `projectName`                     | Project identifier used for resource naming.                                | `ag-aione`        |
| `location`                        | Azure region where resources will be deployed.                              | `austr  aliaeast`   |
| `tags`                            | Key-value pairs for resource tags (from `infra/tags.json`).                 |                   |
| `azureTenantId`                 | Azure Tenant ID, read from environment variables.                           |                   |
| `aspCoreEnvironment`            | ASP.NET Core environment setting.                                           | `Development`     |
| `appServicePlanName`            | Name of the App Service Plan.                                               | `[projectName]-[env]-app`|
| `applicationInsightsName`       | Name of the Application Insights resource.                                  | `[projectName]-[env]-apiapp`|
| `logAnalyticsWorkspaceResourceId` | Resource ID of the Log Analytics Workspace.                                 |  |
| `keyVaultName`                  | Name of the Key Vault.                                                      | `[projectName]-[env]-kv` |
| `storageAccountName`            | Name of the Storage Account.                                                | `agaionedevsto`   |
| `documentIntelligenceServiceName`| Name of the Document Intelligence Service.                                  | `[projectName]-[env]-docintel` |
| `documentIntelligenceEndpoint`  | Endpoint for the Document Intelligence Service.                             | `https://[projectName]-[env]-docintel.cognitiveservices.azure.com/` |
| `openAiName`                    | Name of the OpenAI resource.                                                | `[projectName]-[env]-foundry` |
| `openAiEndpoint`                | Endpoint for the OpenAI resource.                                           | `https://[projectName]-[env]-foundry.openai.azure.com/` |
| `openAiApiVersion`              | API version for the OpenAI resource.                                        | `2024-08-01-preview` |
| `searchServiceName`             | Name of the Azure Cognitive Search Service.                                 | `[projectName]-[env]-search` |
| `serviceBusName`                | Name of the Service Bus.                                                    | `[projectName]-[env]-service-bus` |
| `cosmosDbAccountName`           | Name of the Cosmos DB Account.                                              | `[projectName]-[env]-cosmos` |
| `cosmosDbAccountEndpoint`       | Endpoint for the Cosmos DB Account.                                         | `https://[projectName]-[env]-cosmos.documents.azure.com:443/` |
| `eventGridName`                 | Name of the Event Grid resource.                                            | `[projectName]-[env]-blob-eg` |
| `allowedOrigins`                | List of allowed origins for CORS.                                           | `['https://[projectName]-[env]-webapp.azurewebsites.net']` |
| `deployRoleAssignments`         | Boolean flag to control whether role assignments are deployed for the managed identity. | `true` |
| `networkIsolation`              | Boolean flag to enable or disable network isolation for resources. The endpoint would still be accessible from internet and the app service will be integrated with vnet. | `false` |
| `allowPrivateAccessOnly`        | Specifies whether the app service should be accessible only through private network. | `false` |
| `enableIpRestrictions`          | Enable IP restrictions for the App Service to restrict access to specific IP addresses/ranges. | `false` |
| `allowedIpAddresses`            | Array of allowed IP addresses/ranges for App Service access (e.g., Application Gateway public IP). | `[]` |

### 2.2.6 Deploy Frontend Application

The pipeline will create the Azure resources and also deploy the frontend web application to Azure App Service.

The Frontend application would also use the platform resources provisioned in the previous step.

Update the parameters file to match the environment configuration of the platform resources.

Update the `logAnalyticsWorkspaceResourceId` parameter value to the resource ID of the Log Analytics Workspace created in the previous step. If using an existing Log Analytics Workspace, provide its resource ID.

```plaintext

    infra/
    │
    ├── frontend/
    │   └── main.bicep
    │   └── [env].bicepparam

```

Deployment Pipeline: `Deploy Frontend Web App`

Bicep File: `infra/frontend/webapp.bicep`

Bicep Parameters: `infra/frontend/[env].bicepparam`

| Parameter Name                    | Description                                                                 | Default Value     |
|-----------------------------------|-----------------------------------------------------------------------------|-------------------|
| `environmentName`                 | Name of the deployment environment (e.g., `dev`, `tst`, `uat`, `prod`).     |                   |
| `projectName`                     | Project identifier used for resource naming.                                | `ag-aione`        |
| `location`                        | Azure region where resources will be deployed.                              | `australiaeast`   |
| `tags`                            | Key-value pairs for resource tags (from `infra/tags.json`).                 |                   |
| `logAnalyticsWorkspaceResourceId` | Resource ID of the Log Analytics Workspace.                                 |                   |
| `azureTenantId`                  | Azure Tenant ID, read from environment variables.                           |                   |
| `appServicePlanName`             | Name of the App Service Plan.                                               | `[projectName]-[env]-app`|
|`apiAppName`                   | Name of the API App.                                                        | `[projectName]-[env]-apiapp`|
| `networkIsolation`              | Boolean flag to enable or disable network isolation for resources. The endpoint would still be accessible from internet and the app service will be integrated with vnet. | `false` |
| `allowPrivateAccessOnly`        | Specifies whether the app service should be accessible only through private network. | `false` |
| `enableIpRestrictions`          | Enable IP restrictions for the App Service to restrict access to specific IP addresses/ranges. | `false` |
| `allowedIpAddresses`            | Array of allowed IP addresses/ranges for App Service access (e.g., Application Gateway public IP). | `[]` |

---

### 2.2.7 Deploy Application Gateway (Optional)

1. Update the `logAnalyticsWorkspaceResourceId` parameter value to the resource ID of the Log Analytics Workspace created in the previous step. If using an existing Log Analytics Workspace, provide its resource ID.
2. Create and upload the SSL certificate to the Key Vault. The certificate will be used for securing the Application Gateway.
3. Update the `keyVaultName` and `sslCertificateSecretName` parameters to match the Key Vault and SSL certificate secret name.
4. Update the `webAppServiceName` and `apiAppServiceName` parameters to match the frontend and backend app service names.
5. Update the `virtualNetworkName` and `virtualNetworkSubnetName` parameters to match the virtual network and subnet names where the Application Gateway will be deployed.
6. Run the `Deploy Application Gateway` pipeline to provision the Application Gateway.
7. Update the `VITE_AGILECHAT_API_URL` environment variable in the frontend web app to point to the Application Gateway URL. Redeploy the frontend web app to apply the changes.
8. Configure the app service to allow traffic from the Application Gateway.
    - After deploying the Application Gateway, note the public IP address from the deployment output.
    - Update the frontend and backend parameter files to enable IP restrictions:
    - Redeploy the frontend and backend applications to apply the IP restrictions.

      ```bicep
      
      param allowPrivateAccessOnly = true

      param enableIpRestrictions = true
      param allowedIpAddresses = ['<APPLICATION_GATEWAY_PUBLIC_IP>/32']
      ```

    **Alternative - Manual Configuration**: You can also configure this manually through the Azure Portal:

    - Navigate to the Azure Portal and select the frontend web app.
    - Under **Inbound traffic configuration**, click on the link next to **Public network access**.
    - Select **Enabled from select virtual networks and IP addresses**.
    - Under **Site Access and rules**, add the IP address of the Application Gateway to the allowed IP addresses.
    - Repeat the above steps for the backend API app.

9. Test the Application Gateway by accessing the frontend web app URL. The Application Gateway should route traffic to the frontend web app and backend API app.

```plaintext

    infra/
    │
    ├── platform/
    │   ├── appgw/
    │   │   └── main.bicep
    │   │   └── [env].bicepparam

```

Deployment Pipeline: `Deploy Application Gateway`

Bicep File: `infra/platform/appgw/main.bicep`

Bicep Parameters: `infra/platform/appgw/[env].bicepparam`

| Parameter Name                    | Description                                                                 | Default Value                          |
|-----------------------------------|-----------------------------------------------------------------------------|----------------------------------------|
| `environmentName`                 | Name of the deployment environment (e.g., `dev`, `tst`, `uat`, `prod`).     |                                        |
| `projectName`                     | Project identifier used for resource naming.                                | `ag-aione`                             |
| `location`                        | Azure region where resources will be deployed.                              | `australiaeast`                        |
| `tags`                            | Key-value pairs for resource tags (from `infra/tags.json`).                 |                                        |
| `logAnalyticsWorkspaceResourceId` | Resource ID of the Log Analytics Workspace.                                 |                                        |
| `subnetAddressPrefix`             | Address prefix for the Application Gateway subnet.                          |                                        |
| `keyVaultName`                    | Name of the Key Vault for SSL certificate storage.                          |                                        |
| `sslCertificateSecretName`        | Name of the SSL certificate secret in Key Vault.                            |                                        |
| `webAppServiceName`               | Name of the frontend web app service.                                       |                                        |
| `webAppServiceDomain`             | Custom domain for the frontend web app (leave empty for default).           | *(azurewebsites.net domain)*           |
| `apiAppServiceName`               | Name of the backend API app service.                                        |                                        |
| `apiAppServiceDomain`             | Custom domain for the backend API app (leave empty for default).            | *(azurewebsites.net domain)*           |
| `virtualNetworkSubnetName`        | Name of the subnet where App Gateway will be deployed.                      | `AppGatewaySubnet`                     |
| `enableDiagnostics`               | Enable diagnostic settings.                                                 | `true`                                 |
| `enableWaf`                       | Enable WAF (Web Application Firewall).                                      | `true`                                 |
| `wafMode`                         | WAF mode: Detection or Prevention.                                          | `Detection`                            |
| `wafRuleSetType`                  | WAF rule set type.                                                          | `OWASP`                                |
| `wafRuleSetVersion`               | WAF rule set version.                                                       | `3.2`                                  |

## 3. CI/CD Configuration

### 3.1 GitHub Actions

#### 3.1.2 Configure Permissions for GitHub Actions

To enable GitHub Actions to deploy resources to Azure, you must set up **Federated Credentials** between Azure Entra ID (Entra ID) and the GitHub repository. This allows GitHub Actions workflows to authenticate securely to Azure without storing secrets.

***To set up Federated Credentials:***

1. In the Azure Portal, go to **Azure Entra ID > App registrations** and select the application registration created earlier (e.g., `My Company AI Platform - Dev`).
2. In the left menu, select **Certificates & secrets > Federated credentials**.
3. Click **+ Add credential**.
4. Fill in the form:
    - **Name**: e.g., `GitHubActions-<env>`
    - **Subject identifier**: `repo:<org>/<repo>:environment:<env>` (e.g., `repo:myorg/myrepo:environment:dev`)
5. Click **Add** to save the federated credential.
6. Repeat steps 3-5 for each environment you created in the previous step (e.g., `dev`, `tst`, `uat`, `prod`).

---

#### 3.1.3 Create Environments in GitHub

The deployment is designed to work with multiple environments (e.g., dev, tst, uat, and prod).
Create the required environments in the new project repository on GitHub.

The environments will be used to manage environment-specific configurations and secrets.

***To create environments in GitHub:***

1. Navigate to the new project repository on GitHub.
2. Click on the **Settings** tab.
3. In the left sidebar, select **Environments** under the **Code and automation** section.
4. Click the **New environment** button and enter a name (e.g., `dev`, `tst`, `uat`, `prod`).
5. Repeat step 4 for each environment required.

---

#### 3.1.4 Create Environment Variables in GitHub

Create the following environment variables in each environment created in the previous step.

***To add environment variables to an environment:***

1. In your repository, go to **Settings** > **Environments** and select the environment to configure.
2. Under the **Environment secrets and variables** section, click **Add variable**.
3. Add the required variables.
4. Repeat steps for each environment to support.

---

### 3.2 Azure DevOps

#### 3.2.1 Configure Permissions for Azure DevOps Pipelines

To enable Azure DevOps Pipelines to deploy resources to Azure, create a Service Connection using a Service Principal with appropriate permissions.

Repeat these steps for each environment (e.g., `dev`, `tst`, `uat`, `prod`) as needed.

***To create a Service Connection:***

1. In Azure DevOps, go to **Project Settings > Service connections**.
2. Click **New service connection** > **Azure Resource Manager**.
3. Select **App registration (Manual)**.
4. For **Credential Type**, choose **Workload Identity Federation**.
5. Enter a **Service Connection Name** and **Description** (e.g., `dev-service-connection`).
6. Set **Environment** to **Azure Cloud**.
7. Enter your Azure **Tenant ID**.
8. Click **Next**.

***Configure Federated Credentials in Azure***

1. In the Azure portal, navigate to **Azure Entra ID > App registrations** and select your app registration.
2. In the left menu, select **Certificates & secrets > Federated credentials**.
3. Click **+ Add credential** and choose **Other issuer** as the scenario.
4. Copy the **Issuer URL** and **Subject identifier** from the Azure DevOps service connection setup page and paste them into the corresponding fields in Azure.
5. Provide a **Name** for the federated credential (e.g., `AzureDevOps-<env>`).
6. Click **Add** to save the federated credential.

***Complete Service Connection Setup***

1. Back in Azure DevOps, set the **Scope Level** to **Subscription**.
2. Enter your **Azure Subscription ID** and **Subscription Name**.
3. Enter the **Service Principal Client ID** (from your app registration).
4. (Optional) Select **Grant access permission to all pipelines** if you want all pipelines to use this connection.
5. Click **Save** to create the service connection.

---

#### 3.2.2 Create Environments in Azure DevOps

The deployment is designed to work with multiple environments (e.g., dev, tst, uat, prod).
Create the required environments in your Azure DevOps project.

***To create environments in Azure DevOps:***

1. Go to **Pipelines > Environments**.
2. Click **New environment** and enter a name (e.g., `dev`, `tst`, `uat`, `prod`).
3. Repeat for each environment required.

Environments are used to manage environment-specific configurations, secrets, and approvals.

---

#### 3.2.3 Create Environment Variables in Azure DevOps

Create the following environment variables in each environment created in the previous step.

***To add environment variables:***

1. Go to **Pipelines > Library** in Azure DevOps.
2. Click **+ Variable group**.
3. Name the group exactly as your environment (e.g., `dev`, `tst`, `uat`, `prod`).
4. Add the required variables.
5. Repeat the steps for each environment to support.

---

#### 3.2.4 Create Azure Devops Pipelines

1. In Azure DevOps, go to **Pipelines**.
2. Click **New pipeline** and select your repository.
3. Choose **Existing Azure Pipelines YAML file** and select the YAML file for the pipeline you want to create.
4. Click on **Save** to create the pipeline.

## 4. Appendix

### 4.1 Assign Roles to Backend API Managed Identity

If the deployment was completed with `deployRoleAssignments = false` due to insufficient permissions, you'll need to manually configure the role assignments for the managed identities to enable proper service-to-service authentication.

#### Prerequisites

- **User Access Administrator** or **Owner** role on the Azure subscription or resource group.

The AI-One solution requires the following role assignments for the API App's managed identity:

| Target Resource | Role | Purpose |
|-----------------|------|---------|
| Azure OpenAI | Cognitive Services OpenAI User | Access OpenAI models for chat and embeddings |
| Storage Account | Storage Blob Data Contributor | Read/write access to blob containers |
| Document Intelligence | Cognitive Services User | Process documents using AI |
| Service Bus | Azure Service Bus Data Receiver | Receive messages from queues |
| Service Bus | Azure Service Bus Data Sender | Send messages to queues |
| Service Bus | Azure Service Bus Data Sender | Send blob events to the queue |
| Key Vault | Key Vault Secrets User | Read secrets and connection strings |

**1. Find the API App Managed Identity**

1. Navigate to the Azure Portal and go to your resource group (e.g., `rg-practice-ai-aione-dev`)
2. Find the managed identity resource named `id-<projectName>-<env>-apiapp` (e.g., `id-ag-aione-dev-apiapp`)
3. Click on the managed identity and note the **Object (principal) ID**

**2. Assign OpenAI Access**

1. Navigate to your Azure OpenAI resource (e.g., `ag-aione-dev-foundry`)
2. Go to **Access control (IAM)** in the left menu
3. Click **+ Add** > **Add role assignment**
4. On the **Role** tab, search for and select **Cognitive Services OpenAI User**
5. Click **Next**
6. On the **Members** tab, select **Managed identity**
7. Click **+ Select members**
8. Choose your subscription and **User-assigned managed identity**
9. Select the API app managed identity (`id-ag-aione-dev-apiapp`)
10. Click **Select** then **Review + assign**

**3. Assign Storage Account Access**

1. Navigate to your Storage Account (e.g., `agaionedevsto`)
2. Go to **Access control (IAM)**
3. Click **+ Add** > **Add role assignment**
4. Select **Storage Blob Data Contributor** role
5. Follow steps 6-10 from Step 2 to assign to the API app managed identity

**4. Assign Document Intelligence Access**

1. Navigate to your Document Intelligence resource (e.g., `ag-aione-dev-docintel`)
2. Go to **Access control (IAM)**
3. Click **+ Add** > **Add role assignment**
4. Select **Cognitive Services User** role
5. Follow steps 6-10 from Step 2 to assign to the API app managed identity

**5. Assign Service Bus Access (Receiver)**

1. Navigate to your Service Bus namespace (e.g., `ag-aione-dev-service-bus`)
2. Go to **Access control (IAM)**
3. Click **+ Add** > **Add role assignment**
4. Select **Azure Service Bus Data Receiver** role
5. Follow steps 6-10 from Step 2 to assign to the API app managed identity

**6. Assign Service Bus Access (Sender)**

1. In the same Service Bus namespace
2. Click **+ Add** > **Add role assignment** again
3. Select **Azure Service Bus Data Sender** role
4. Follow steps 6-10 from Step 2 to assign to the API app managed identity

**7. Assign Key Vault Access**

1. Navigate to your Key Vault (e.g., `ag-aione-dev-kv`)
2. Go to **Access control (IAM)**
3. Click **+ Add** > **Add role assignment**
4. Select **Key Vault Secrets User** role
5. Follow steps 6-10 from Step 2 to assign to the API app managed identity

#### Configure Event Grid System Topic Role Assignment

**1. Find the Event Grid System Topic**

1. Navigate to your Event Grid System Topic (e.g., `ag-aione-dev-blob-eg`)
2. Go to **Identity** in the left menu
3. Ensure **System assigned** identity is **On** and note the **Object (principal) ID**

**2. Assign Service Bus Sender Access**

1. Navigate to your Service Bus namespace (e.g., `ag-aione-dev-service-bus`)
2. Go to **Access control (IAM)**
3. Click **+ Add** > **Add role assignment**
4. Select **Azure Service Bus Data Sender** role
5. Click **Next**
6. On the **Members** tab, select **Managed identity**
7. Click **+ Select members**
8. Choose your subscription and **System-assigned managed identity**
9. Select **Event Grid System Topic** and find your topic
10. Click **Select** then **Review + assign**

#### PowerShell Automation Script

For automated role assignment configuration, use the provided PowerShell script `infra/scripts/configure-role-assignments.ps1`.

**Prerequisites:**

Before running the script, install the required Azure PowerShell modules. Run PowerShell as **Administrator** and execute:

```powershell
# Install all required Azure PowerShell modules
Install-Module -Name Az.Accounts, Az.Resources, Az.ManagedServiceIdentity, Az.Storage, Az.KeyVault, Az.ServiceBus, Az.CognitiveServices, Az.EventGrid -Scope AllUsers -Repository PSGallery -Force
```

**Usage:**

```powershell
# Navigate to the scripts directory
cd infra/scripts

# Configure role assignments with resource names
.\configure-role-assignments.ps1 `
    -SubscriptionId "9221a966-ce17-4b76-a348-887f234a827a"
    -ResourceGroupName "rg-practice-ai-aione-dev" `
    -ApiAppManagedIdentityName "id-ag-aione-dev-apiapp" `
    -OpenAIServiceName "ag-aione-dev-foundry" `
    -StorageAccountName "agaionedevsto" `
    -DocumentIntelligenceServiceName "ag-aione-dev-docintel" `
    -ServiceBusName "ag-aione-dev-service-bus" `
    -KeyVaultName "ag-aione-dev-kv" `
    -EventGridTopicName "ag-aione-dev-blob-eg"
```
