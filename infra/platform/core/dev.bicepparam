using './main.bicep'

param environmentName = 'dev'
var sharedVariables = loadJsonContent('../../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../../tags.json')

param logAnalyticsWorkspaceResourceId = '/subscriptions/a8d44b38-6c3d-459c-b6c5-8bc173941b44/resourcegroups/ms-rg-aione_dev1-ae/providers/microsoft.operationalinsights/workspaces/ms-aione-dev-la'

param azureClientId = readEnvironmentVariable('AZURE_CLIENT_ID')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param networkIsolation = false
