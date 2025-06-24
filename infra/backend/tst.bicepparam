using './apiapp.bicep'

param environmentName = 'tst'
param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'Test'
param appServicePlanName = 'ag-aione-tst-app'
param applicationInsightsName = 'ag-aione-tst-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-tst-la'
param keyVaultName = 'ag-aione-tst-kv'
param storageName = 'agaionetststo'
param storageAccountName = 'agaionetststo'
param formRecognizerName = 'ag-aione-tst-form'
param openAiName = 'ag-aione-tst-aillm'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aione-tst-search'
param serviceBusName = 'ag-aione-tst-service-bus'
param cosmosDbAccountName = 'ag-aione-tst-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aione-tst-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aione-tst-blob-eg'

param allowedOrigins = ['https://ag-aione-tst-webapp.azurewebsites.net']

param adminEmailAddresses = ['adam-stephensen@agile-analytics.com.au']
