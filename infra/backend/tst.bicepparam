using './apiapp.bicep'

param environmentName = 'tst'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Test'
param appServicePlanName = 'ag-aione-tst-app'
param applicationInsightsName = 'ag-aione-tst-apiapp'

param logAnalyticsWorkspaceResourceId = '<REPLACE_WITH_YOUR_LOG_ANALYTICS_WORKSPACE_ID>'

param keyVaultName = 'ag-aione-tst-kv'
param storageAccountName = 'agaionetststo'
param documentIntelligenceServiceName = 'ag-aione-tst-docintel'
param documentIntelligenceEndpoint = 'https://ag-aione-tst-docintel.cognitiveservices.azure.com/'
param openAiName = 'ag-aione-tst-foundry'
param openAiEndpoint = 'https://ag-aione-tst-foundry.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aione-tst-search'
param serviceBusName = 'ag-aione-tst-service-bus'
param cosmosDbAccountName = 'ag-aione-tst-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aione-tst-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aione-tst-blob-eg'

param aiFoundryAccountName = 'ag-aione-tst-foundry'
param aiFoundryProjectName = 'ag-aione-tst-prj'
param aiFoundryProjectEndpoint = 'https://ag-aione-tst-foundry.services.ai.azure.com/api/projects/ag-aione-tst-prj'


param allowedOrigins = ['https://ag-aione-tst-webapp.azurewebsites.net']
