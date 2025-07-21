using './main.bicep'

param environmentName = 'prod'
var sharedVariables = loadJsonContent('../../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../../tags.json')

param logAnalyticsWorkspaceResourceId = '<REPLACE_WITH_YOUR_LOG_ANALYTICS_WORKSPACE_ID>'

param azureClientId = readEnvironmentVariable('AZURE_CLIENT_ID')
param azureTenantId = readEnvironmentVariable('AZURE_TENANT_ID')

param networkIsolation = false
