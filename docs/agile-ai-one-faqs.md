# Agile Chat FAQS

## Who is AgileChat Built for ?
AgileChat is aimed at enterprise, public sector and commercial customers like government agencies, universities, finaincial institutions and retail enterprises. 

## What are the challenges that AgileChat addresses ? 

The Challenges that we see customers have when building AI projects are

- Many customers have built several AI point solutions, either using AI solution accelerators provided by Microsoft, by using the AI Studio portal or using Microsoft Copilot. Because each of these AI Assistants is developed separately there is no common authentication/administration/security layer. Features that are built and customised for one AI application are not available in other AI applications. Maintenance and management of these solutions is complex. Managing data sources for these assistants is complex. In the age of agent orchestration it is complex to create an orchtestrator that can incorporate and co-ordinate these discrete AI applications as agents in a broader AI ecosystem.
- If every time you want to do a new poc you need to do a new copilot or accelerator deployment.. each one is quick, but your complexity and mamagement overhead is goign to explode. 


## What featuers does AgileChat provide
It has the following features.
	
    - AgileChat supports creating AI Agents and chatbots with custom system prompts that connect to enterprise data sources e.g. databases, APIs, data platforms like Azure Fabric
	- AgileChat allows users in the user interface to create new AI Search Indexes, Upload documents to those index and create AI Agents and Chatbots over those AI Search Indexes by customising the system prompt and the various LLM parameters like temperature, top p etc.
	- AgileChat supports end users creating their own AI Chat Bots with custom system prompts, temperature top p etc.
	- AgileChat enables role based access control so different groups within the organisation can  only access their allocated AI Assistants and AI Search Indexes

### Key Feature - Private Chat

Agile Chat allows organisations to deploy a private ChatGPT-like chat instance in their Azure Subscription with a familiar user experience and the added capabilities of chatting over your data and files.
The key benefits of AgileChat over a public or shared OpenAI/ChatGPT solution include are:
•	Private: The solution is deployed into the DPC Azure tenancy, allowing complete isolation and security controlled to a specific Azure Entra tenant.
•	Controlled: Network traffic can be fully isolated to the DPC environment and routed through appropriate firewalls and other access control mechanisms. 
•	Configurability : Customisation has been added with DPC data sources, the ability to securely upload specific files and the future functionality to connect to other internal data sources or systems.



## What data sources does AgileChat support

AI is only as capable as the data it has access to. 

AgileChat integrates across a broad number of datasources for different customers. 

1. Automated enterprise data ingestion

A common pattern for data ingestion for enteprises is to provide a blob storage container to the organisation.
The data team at the organisation can then export content from their existing systems (e.g. policy documents, content from CMS systems) into the speicifed storage file, from where it is automatically ingested and indexed into Azure AI Search, making it availble to AI assistants

2. Manual upload via the AgileChat UI

AgileChat includes capabilities to create new "Containers". A Container in the AgileChat ui is an abtraction over Azure AI Search Indexes. 


4. SharePoint integration

Connecting directly to Sharepoint to access information is a feature that is currently in development. 



## Comparisons to other solutions

### How is Agile Chat different to Point Solutions like AI Accelerators or Low Code/No Code AI tools like CoPilot Studio ? 

1. Customizability and Flexibility
- Low-code/no-code tools like Copilot Studio are designed for rapid prototyping and general use cases, but they often fall short for complex, enterprise-specific requirements.
- AgileChat offers deep enterprise-specific customization to ensure the solution aligns precisely with the organization's unique processes, data architecture, and compliance needs.

2. Scalability and Integration
- Challenges with Copilot Studio: While it provides ease of integration, scaling multiple agents with common authentication, administration, and security layers across departments becomes challenging.
- AgileChat scales seamlessly by consistently integrating AI Assistants with enterprise-grade authentication and role-based access control.
- AgileChat handles orchestration of multiple AI agents as part of a broader AI ecosystem, something generic tools don’t inherently provide.


3. Operational Efficiency
Challenges with AI point solutions: Organizations often face fragmented solutions, leading to duplicated efforts in authentication, security, and administration.
AgileChat eliminates this duplication:
- Unified orchestration across AI Assistants.
- Streamlined maintenance and reduced complexity.
- Shared features across all AI applications.

4. Enterprise Data Handling
- Data complexity: Copilot Studio may connect to structured and unstructured data but does not provide consistent indexing, role-based access, and data source management that enterprises require.
- AgileChat provides: AI Search Index creation for fine-grained data control, centralized management of enterprise data sources and secure partitioning of data and AI models between departments.

5. Ownership and Control
Vendor lock-in: Off-the-shelf solutions tie organizations to a specific vendor’s roadmap and constraints.
AgileChat:
- Offers full control over features, roadmap, and data sovereignty.
- Enables rapid adaptation to changing enterprise needs, beyond the capabilities of generic tools.

6. Long-term ROI
Copilot has a per user pricing that for public sector organisation, enterprises and large organisation can make CoPilot inaccessible. 
Lack of flexibility and high maintenance costs for enterprise-scale deployment can make it more costly over time.
AgileChat:
- A tailored solution with predictable maintenance and upgrade paths.
- Greater long-term value as it evolves alongside enterprise needs.


## Technical FAQs

1. What technologies does AgileChat leverage ? 

Azure Chat is built on Azure OpenAI, Azure AI Search and other Azure services.

The front end application is a React web application that is hosted on an Azure Web App. 
The primary API is a .NET Core minimal API hosted on an Azure Web App. 
Both web apps can share an App Service Plan.

CosmosDB is the primary database utilised.

Work is currently in progress to support a range of other models as well as OpenAI.


2. What Authorisation methods are available: 

The system utilises three roles for users: Admins, Content Managers, and Users

Examples of role providers: 
i. Default Authorization Provider - all users are admins
ii. Simple Authorization Provider 
- users are added to one of the three roles via their email address in a simple UI in AgileChat
- this configuration is stored in CosmosDB
iii. Entra Authorization Provider 
iv. MS Graph Authorization Provider 
v. External API Authorization Provider - (built for uts) they supply generic endpoints that provide a particular interface that lets us query roles


3. What components are used in the application ? 

The role of each of the components in the diagram above is described here:
Azure Open AI : Large Language Model used to understand and generate text queries and responses.
App Service Plan: Hosts the web site application code and communicates with the back-end services.
Azure Cosmos DB: Used to store the chat history and settings of every chat associated to users.
Document Intelligence: Used to process and embed/vectorize documents uploaded in a chat thread.
Key vault: Stores secrets securely for the web application, event grid and/or APIM instances to access.
Speech Service: Used by the web application for text to speech/speech to text integration in the chat sessions (optional)
Search service: Azure AI Search instance to store vectorized chunked indexes when files are uploaded in chat threads and uploaded files in blob storage folders. Used for RAG querying in chat threads and returning associated citations.
Storage account: Used to store images generated by AI and files uploaded by the user into folders for automatic pipeline ingestion.
Event Grid System Topic: Used to subscribe and listen to changed blob storage folder events to automatically trigger a webhook call to the web application to begin indexing the files in Azure AI Search.
APIM: Gateway to provide optional connection to Azure OpenAI API’s and authenticate using AD JWT tokens instead of keys.
EntraID: Used to allow users to authenticate and log into the web application using their Microsoft credentials based on their AD domain.


## Security details 

### Security Considerations
Azure AD Integration: The chat solution uses Azure Active Directory (Azure AD) for secure user authentication and single sign-on. This enables role-based access control (RBAC), allowing only authorized users to access the application.
Token-Based Access: Access tokens are used to secure API calls between components, following OAuth 2.0 and OpenID Connect protocols. Each session token has a set expiration to limit access duration and requires reauthentication when expired.

### Data Protection
Encryption at Rest: CosmosDB and any other data stores use AES-256 encryption for data at rest to safeguard sensitive information. All backups are also encrypted following Azure standards.
Encryption in Transit: All data transmitted between services is encrypted using TLS 1.2 or above, protecting data integrity and confidentiality as it flows across Azure services and to the end-user’s device.
VNET: private networks are to be installed before going live to production to ensure network access is restricted to only within internal organisation use.
Key Management: All keys and secrets required by the solution are stored securely in Azure Key Vault. Access to the vault is strictly controlled through Azure AD roles, and each component only has access to the keys it needs.

### Role-Based Access Control (RBAC)
Roles are defined to restrict access at different levels (e.g., admin, content administration, user) and ensure users have only the minimum necessary permissions. RBAC currently used for NAVI is only user & admin Roles. To increase the privilege of a user to become an administrator, set the ADMIN_EMAIL_ADDRESS environment configuration in the web app to be a comma separated list of email addresses that require admin level permissions. Admins are allowed to view logs of chats and publish global level prompts and personas.




