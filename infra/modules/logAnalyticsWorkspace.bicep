@description('Log Analytics Workspace name')
param name string

@description('Azure region for resource deployment')
param location string

@description('Resource tags')
param tags object

module logAnalyticsWorkspace 'br/public:avm/res/operational-insights/workspace:0.11.2' = {
  name: take('${name}-log-analytics-deployment', 64)
  params: {
    name: name
    location: location
    tags: tags
    skuName: 'PerGB2018'
    dataRetention: 30
    // publicNetworkAccessForIngestion: 'Disabled'
    // publicNetworkAccessForQuery: 'Disabled'
    dailyQuotaGb: 10
  }
}

output name string = logAnalyticsWorkspace.outputs.name
output resourceId string = logAnalyticsWorkspace.outputs.resourceId
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.outputs.logAnalyticsWorkspaceId
