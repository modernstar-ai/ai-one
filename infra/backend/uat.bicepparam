using './apiapp.bicep'

param environmentName = 'uat'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param aspCoreEnvironment = 'UAT'
param appServicePlanName = 'ag-aionev12-uat-app'
param applicationInsightsName = 'ag-aionev12-uat-apiapp'
param logAnalyticsWorkspaceResourceId = '/subscriptions/9221a966-ce17-4b76-a348-887f234a827a/resourcegroups/rg-practice-ai-aione-private-dev/providers/microsoft.operationalinsights/workspaces/ag-aionev12-uat-la'

param keyVaultName = 'ag-aionev12-uat-kv'
param storageAccountName = 'agaionev12uatsto'
param maxServiceBusQueueMessageSizeInKilobytes = 1024
param documentIntelligenceServiceName = 'ag-aionev12-uat-docintel'
param documentIntelligenceEndpoint = 'https://ag-aionev12-uat-docintel.cognitiveservices.azure.com/'
param openAiName = 'ag-aionev12-uat-foundry'
param openAiEndpoint = 'https://ag-aionev12-uat-foundry.openai.azure.com/'
param openAiApiVersion = '2024-08-01-preview'
param searchServiceName = 'ag-aionev12-uat-search'
param serviceBusName = 'ag-aionev12-uat-service-bus'
param cosmosDbAccountName = 'ag-aionev12-uat-cosmos'
param cosmosDbAccountEndpoint = 'https://ag-aionev12-uat-cosmos.documents.azure.com:443/'
param eventGridName = 'ag-aionev12-uat-blob-eg'

param allowedOrigins = ['https://ag-aionev12-uat-webapp.azurewebsites.net']

param networkIsolation = true
param virtualNetworkName = 'ag-aionev12-uat-vnet'
param privateEndpointsSubnetName = 'PrivateEndpointsSubnet'
param allowPrivateAccessOnly = true

// Optional: Enable IP restrictions to allow access only from Application Gateway
// Uncomment and update the IP address after deploying the Application Gateway
// param enableIpRestrictions = true
// param allowedIpAddresses = ['<APPLICATION_GATEWAY_PUBLIC_IP>/32']

param enableIpRestrictions = true
param allowedIpAddresses = ['68.218.105.22/32']
