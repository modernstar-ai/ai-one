using './webapp.bicep'

param environmentName = 'dev'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ag-aione-dev-app'
param apiAppName = 'wsu-app-dev-apiapp'
param logAnalyticsWorkspaceResourceId = '/subscriptions/9221a966-ce17-4b76-a348-887f234a827a/resourcegroups/rg-practice-ai-aione-dev/providers/microsoft.operationalinsights/workspaces/ag-aione-dev-la'

