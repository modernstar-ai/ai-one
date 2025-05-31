@description('Name of the Azure OpenAI resource.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('SKU for Azure OpenAI resource')
param skuName string = 'S0'

@description('Optional. Array of deployments about cognitive service accounts to create.')
param deployments array = []

// @export()
// @description('The type for a cognitive services account deployment.')
// type deploymentType = {
//   @description('Optional. Specify the name of cognitive service account deployment.')
//   name: string?

//   @description('Required. Properties of Cognitive Services account deployment model.')
//   model: {
//     @description('Required. The name of Cognitive Services account deployment model.')
//     name: string

//     @description('Required. The format of Cognitive Services account deployment model.')
//     format: string

//     @description('Required. The version of Cognitive Services account deployment model.')
//     version: string
//   }

//   @description('Optional. The resource model definition representing SKU.')
//   sku: {
//     @description('Required. The name of the resource model definition representing SKU.')
//     name: string

//     @description('Optional. The capacity of the resource model definition representing SKU.')
//     capacity: int?

//     @description('Optional. The tier of the resource model definition representing SKU.')
//     tier: string?

//     @description('Optional. The size of the resource model definition representing SKU.')
//     size: string?

//     @description('Optional. The family of the resource model definition representing SKU.')
//     family: string?
//   }?

//   @description('Optional. The name of RAI policy.')
//   raiPolicyName: string?

//   @description('Optional. The version upgrade option.')
//   versionUpgradeOption: string?
// }

resource cognitiveService 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: name
  location: location
  tags: tags
  kind: 'OpenAI'
  properties: {
    customSubDomainName: name
    publicNetworkAccess: 'Enabled'
  }
  sku: {
    name: skuName
  }
}

@batchSize(1)
resource cognitiveService_deployments 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = [
  for (deployment, index) in (deployments ?? []): {
    parent: cognitiveService
    name: deployment.?name ?? '${name}-deployments'
    properties: {
      model: deployment.model
      raiPolicyName: deployment.?raiPolicyName
      versionUpgradeOption: deployment.?versionUpgradeOption
    }
    sku: deployment.?sku
  }
]

output name string = cognitiveService.name
output endpoint string = cognitiveService.properties.endpoint
output resourceId string = cognitiveService.id
