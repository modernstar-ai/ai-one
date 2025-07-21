using './webapp.bicep'

param environmentName = 'uat'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ag-aionev12-uat-app'
param apiAppName = 'ag-aionev12-uat-apiapp'
param logAnalyticsWorkspaceResourceId = '/subscriptions/9221a966-ce17-4b76-a348-887f234a827a/resourcegroups/rg-practice-ai-aione-private-dev/providers/microsoft.operationalinsights/workspaces/ag-aionev12-uat-la'

param networkIsolation = true
param virtualNetworkName = 'ag-aionev12-uat-vnet'
param appServiceSubnetName = 'AppServiceSubnetV2'
