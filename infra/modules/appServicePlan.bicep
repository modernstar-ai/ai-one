@description('App Service Plan name')
param appServicePlanName string

@description('Azure region for resource deployment')
param location string

@description('Resource tags')
param tags object

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  properties: {
    reserved: true
  }
  sku: {
    name: 'P0v3'
    tier: 'Premium0V3'
    size: 'P0v3'
    family: 'Pv3'
    capacity: 1
  }
  kind: 'linux'
}

output appServicePlanName string = appServicePlan.name
output appServicePlanId string = appServicePlan.id
