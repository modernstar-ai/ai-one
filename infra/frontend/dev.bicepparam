using './webapp.bicep'

param environmentName = 'dev'

param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ag-aione-dev-app'
param apiAppName = 'ag-aione-dev-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-dev-la'
