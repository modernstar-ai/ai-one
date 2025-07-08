using './apiapp.bicep'

param environmentName = 'dev'
param projectName = readEnvironmentVariable('PROJECT_NAME', 'ms-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Development'
param appServicePlanName = 'ms-aione-dev-app'
param applicationInsightsName = 'ms-aione-dev-apiapp'
param logAnalyticsWorkspaceName = 'ms-aione-dev-la'
param keyVaultName = 'ms-aione-dev-kv'
param storageName = 'msaionedevsto'
param storageAccountName = 'msaionedevsto'
param documentIntelligenceServiceName = 'ms-aione-dev-docintel'
param documentIntelligenceEndpoint = 'https://ms-aione-dev-docintel.cognitiveservices.azure.com/'
param openAiName = 'ms-aione-dev-foundry'
param openAiEndpoint = 'https://ms-aione-dev-foundry.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ms-aione-dev-search'
param serviceBusName = 'ms-aione-dev-service-bus'
param cosmosDbAccountName = 'ms-aione-dev-cosmos'
param cosmosDbAccountEndpoint = 'https://ms-aione-dev-cosmos.documents.azure.com:443/'
param eventGridName = 'ms-aione-dev-blob-eg'

param allowedOrigins = ['https://ms-aione-dev-webapp.azurewebsites.net']
param adminEmailAddresses = ['']
