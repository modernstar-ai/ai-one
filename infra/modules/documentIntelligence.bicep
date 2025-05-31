@description('Name of the Document Intelligence resource.')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

resource documentIntelligence 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: name
  location: location
  tags: tags
  kind: 'FormRecognizer'
  properties: {
    customSubDomainName: name
    publicNetworkAccess: 'Enabled'
  }
  sku: {
    name: 'S0'
  }
}

output name string = documentIntelligence.name
output id string = documentIntelligence.id
output endpoint string = documentIntelligence.properties.endpoint
