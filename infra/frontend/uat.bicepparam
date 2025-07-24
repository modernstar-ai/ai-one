using './webapp.bicep'

param environmentName = 'uat'
var sharedVariables = loadJsonContent('../shared-parameters.json')

param projectName = readEnvironmentVariable('PROJECT_NAME', sharedVariables.projectName)
param location = readEnvironmentVariable('AZURE_LOCATION', sharedVariables.location)
param tags = loadJsonContent('../tags.json')

param appServicePlanName = 'ag-aionev14-uat-app'
param apiAppName = 'ag-aionev14-uat-apiapp'
param logAnalyticsWorkspaceResourceId = '/subscriptions/9221a966-ce17-4b76-a348-887f234a827a/resourcegroups/rg-practice-ai-aione-private-dev/providers/microsoft.operationalinsights/workspaces/ag-aionev14-uat-la'

param networkIsolation = true
param virtualNetworkName = 'ag-aionev14-uat-vnet'
param appServiceSubnetName = 'AppServiceSubnet'
param privateEndpointsSubnetName = 'PrivateEndpointsSubnet'
param allowPrivateAccessOnly = true

// Optional: Enable IP restrictions to allow access only from Application Gateway
// Uncomment and update the IP address after deploying the Application Gateway
// param enableIpRestrictions = true
// param allowedIpAddresses = ['<APPLICATION_GATEWAY_PUBLIC_IP>/32']

param enableIpRestrictions = true
param allowedIpAddresses = ['68.218.105.22/32']
