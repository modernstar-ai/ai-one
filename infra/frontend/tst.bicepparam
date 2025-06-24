using './webapp.bicep'

param environmentName = 'tst'

param projectName = readEnvironmentVariable('PROJECT_NAME', 'ag-aione')
param location = readEnvironmentVariable('AZURE_LOCATION', 'australiaeast')
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ag-aione-tst-app'
param apiAppName = 'ag-aione-tst-apiapp'
param logAnalyticsWorkspaceName = 'ag-aione-tst-la'
