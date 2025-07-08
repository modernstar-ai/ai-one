# Application Registration Guide

Application registration is required to enable the following capabilities:

- **User and App Authentication**: Authenticate both users and applications with Azure Active Directory.
- **API Access**: Grant the application permission to access Microsoft Graph and other protected APIs.
- **CI/CD Integration**: Allow deployment pipelines to authenticate and deploy resources securely as part of automated workflows.

In the next section, the guide will walk you through the steps to set up the application registration required for the AI-One solution.

1. Register a new application in Azure.
2. Grant permissions to developers to manage the application registration.
3. Expose an API and define scopes (required for backend applications to allow secure client access).
4. Provide API access to client applications.
5. Set up additional API permissions for role providers (if applicable).

---

## 1. Register a New Application

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

---

## 2. Assign Owners

1. In the app registration, navigate to **Manage > Owners**.
2. Add required developers (e.g., `Adam.Stephensen@agile-insights.com.au`) as owners. This will allow them to configure settings and manage credentials after deployment.

---

## 3. Expose an API (for Backend Application)

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

## 4. Provide API access to Client Applications

AI-One will use the same client ID for both the API and web app.

1. To enable a client application to access this API, go to the **Expose an API** section of your app registration.
    - Click **Add a client application**.
    - Enter the **Client Application ID** you noted earlier.
    - Click **Add application** button.
2. Next, grant the `User.Read` scope to the client application:
    - Navigate to the **API Permissions** tab of the App Registration.
    - Click **Add a permission**.
    - Select **APIs my organization uses**.
    - Start typing the name of the App Registration (the one you just created).
    - Select the App Registration from the list.
    - Choose **Delegated Permissions**.
    - Type `User.Read` into the "Select permissions" search field.
    - Select the `User.Read` Permission.
    - Click the **Add Permission** button.

This will allow the client application to authenticate and access the API using the defined scopes.

---

## 5. Additional API Permissions for Role Providers

AgileOne supports multiple Role Providers which restrict access to functionality and data based on the roles assigned to authenticated users.

- **Default Role Provider**: All authenticated users have access.
- **App Setting Role Provider**: Admins specified via app configuration.
- **Custom Role Provider**: Roles mapped via SQL Server (CSV import).
- **Standard Role Provider**: Roles stored in CosmosDB.
- **Entra Role Provider**: Roles queried from Entra via Microsoft Graph API.

Implementing any role provider other than the **Default Role Provider** requires custom development. If you plan to use the **Entra Role Provider**, you must grant additional Microsoft Graph API permissions to your application registration:

- **Microsoft Graph**:
    - `User.Read`
    - `User.Read.All`

These permissions enable the application to query user roles and directory information from Microsoft Entra ID. Be sure to grant admin consent for these permissions in the Azure Portal to ensure proper access.

---
