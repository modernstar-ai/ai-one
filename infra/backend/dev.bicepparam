using './apiapp.bicep'

param environmentName = 'dev'
param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Development'
param appServicePlanName = 'ag-aione-dev-app'
param applicationInsightsName = 'ag-aione-dev-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-dev-la'
param keyVaultName = 'ag-aione-dev-kv'
param storageName = 'agaionedevsto'
param storageAccountName = 'agaionedevsto'
param documentIntelligenceServiceName = 'ag-aione-dev-docintel'
param documentIntelligenceEndpoint = 'https://ag-aione-dev-docintel.cognitiveservices.azure.com/'
param openAiName = 'ag-aione-dev-foundry'
param openAiEndpoint = 'https://ag-aione-dev-foundry.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aione-dev-search'
param serviceBusName = 'ag-aione-dev-service-bus'
param cosmosDbAccountName = 'ag-aione-dev-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aione-dev-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aione-dev-blob-eg'

param allowedOrigins = ['https://ag-aione-dev-webapp.azurewebsites.net']

param adminEmailAddresses = ['adam-stephensen@agile-analytics.com.au']
