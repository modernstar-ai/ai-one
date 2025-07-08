@description('Name of the App Service Plan.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

module appServicePlan 'br/public:avm/res/web/serverfarm:0.4.1' = {
  name: take('${take(toLower(name), 24)}-serverfarm-deployment', 64)
  params: {
    name: name
    location: location
    tags: tags
    kind: 'linux'
    skuCapacity: 1
    skuName: 'P1v3'
    zoneRedundant: false
  }
}

output resourceId string = appServicePlan.outputs.resourceId
output name string = appServicePlan.outputs.name
