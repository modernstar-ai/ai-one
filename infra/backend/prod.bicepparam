using './apiapp.bicep'

param environmentName = 'prod'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Production'
param appServicePlanName = 'ag-aione-prod-app'
param applicationInsightsName = 'ag-aione-prod-apiapp'

param logAnalyticsWorkspaceResourceId = '<REPLACE_WITH_YOUR_LOG_ANALYTICS_WORKSPACE_ID>'

param keyVaultName = 'ag-aione-prod-kv'
param storageAccountName = 'agaioneprdsto'
param documentIntelligenceServiceName = 'ag-aione-prod-docintel'
param documentIntelligenceEndpoint = 'https://ag-aione-prod-docintel.cognitiveservices.azure.com/'
param openAiName = 'ag-aione-prod-foundry'
param openAiEndpoint = 'https://ag-aione-prod-foundry.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aione-prod-search'
param serviceBusName = 'ag-aione-prod-service-bus'
param cosmosDbAccountName = 'ag-aione-prod-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aione-prod-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aione-prod-blob-eg'
param foundryProjectEndpoint = 'https://ag-aione-prod-foundry.services.ai.azure.com/api/projects/ag-aione-prod-prj'

param allowedOrigins = ['https://ag-aione-prod-webapp.azurewebsites.net']
