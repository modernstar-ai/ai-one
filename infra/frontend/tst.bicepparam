using './webapp.bicep'

param environmentName = 'tst'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ag-aione-tst-app'
param apiAppName = 'ag-aione-tst-apiapp'

param logAnalyticsWorkspaceResourceId = '<REPLACE_WITH_YOUR_LOG_ANALYTICS_WORKSPACE_ID>'
