using './apiapp.bicep'

param environmentName = 'dev'
param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Development'
param appServicePlanName = 'ag-aione-v2-dev-app'
param applicationInsightsName = 'ag-aione-v2-dev-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-v2-dev-la'
param keyVaultName = 'ag-aione-v2-dev2-kv'
param storageName = 'agaionev2devsto'
param storageAccountName = 'agaionev2devsto'
param formRecognizerName = 'ag-aione-v2-dev-form'
param openAiName = 'ag-aione-v2-dev-aillm'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aione-v2-dev-search'
param serviceBusName = 'ag-aione-v2-dev-service-bus'
param cosmosDbAccountName = 'ag-aione-v2-dev-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aione-v2-dev-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aione-v2-dev-blob-eg'

param allowedOrigins = ['https://ag-aione-v2-dev-webapp.azurewebsites.net']

param adminEmailAddresses = ['adam-stephensen@agile-analytics.com.au']
