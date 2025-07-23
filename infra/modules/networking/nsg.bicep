@description('Azure region for resource deployment')
param location string

@description('Network Security Group name')
param nsgName string

@description('Security rules for the NSG')
param securityRules array

@description('Resource tags')
param tags object = {}

// Network Security Group
resource nsg 'Microsoft.Network/networkSecurityGroups@2024-03-01' = {
  name: nsgName
  location: location
  tags: tags
  properties: {
    securityRules: securityRules
  }
}

// Outputs
output nsgId string = nsg.id
output nsgName string = nsg.name
