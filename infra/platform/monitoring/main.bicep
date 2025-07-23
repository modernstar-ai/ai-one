targetScope = 'resourceGroup'

@description('Primary location for all resources')
param location string = resourceGroup().location

@description('The name of the solution.')
@minLength(3)
@maxLength(12)
param projectName string

@description('The type of environment. e.g. local, dev, uat, prod.')
@minLength(1)
@maxLength(4)
param environmentName string

@description('Tags to apply to all resources.')
param tags object = {}

@description('Resource prefix for naming resources')
param resourcePrefix string = toLower('${projectName}-${environmentName}')

@description('Log Analytics Workspace name')
param logAnalyticsWorkspaceName string = toLower('${resourcePrefix}-la')

module logAnalyticsWorkspaceModule '../../modules/logAnalyticsWorkspace.bicep' = {
  name: 'logAnalyticsWorkspaceModule'
  params: {
    name: logAnalyticsWorkspaceName
    location: location
    tags: tags
  }
}

output logAnalyticsWorkspaceName string = logAnalyticsWorkspaceModule.outputs.name
output logAnalyticsWorkspaceId string = logAnalyticsWorkspaceModule.outputs.logAnalyticsWorkspaceId
output logAnalyticsWorkspaceResourceId string = logAnalyticsWorkspaceModule.outputs.resourceId
