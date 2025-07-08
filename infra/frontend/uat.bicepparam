using './webapp.bicep'

param environmentName = 'uat'

param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ag-aione-uat-app'
param apiAppName = 'ag-aione-uat-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-uat-la'
