@description('Log Analytics Workspace name')
param logAnalyticsWorkspaceName string

@description('Azure region for resource deployment')
param location string

@description('Resource tags')
param tags object

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  tags: tags
  location: location
}

output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
