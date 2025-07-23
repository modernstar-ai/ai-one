using './webapp.bicep'

param environmentName = 'prod'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ms-aione-prod-app'
param apiAppName = 'ms-aione-prod-apiapp'

param logAnalyticsWorkspaceResourceId = '/subscriptions/a8d44b38-6c3d-459c-b6c5-8bc173941b44/resourcegroups/ms-rg-aione_prd1-ae/providers/microsoft.operationalinsights/workspaces/ms-aione-prod-la'

