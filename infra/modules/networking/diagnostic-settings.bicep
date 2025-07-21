@description('Log Analytics workspace ID for diagnostics')
param logAnalyticsWorkspaceId string

@description('Target resource name for diagnostics')
param targetResourceName string

@description('Resource type for diagnostics configuration')
@allowed(['nsg', 'vnet', 'bastion', 'publicip'])
param resourceType string

@description('Diagnostic settings name')
param diagnosticSettingsName string = 'diagnosticSettings'

// NSG logs
var nsgLogs = [
  {
    category: 'NetworkSecurityGroupEvent'
    enabled: true
  }
  {
    category: 'NetworkSecurityGroupRuleCounter'
    enabled: true
  }
]

// VNet logs and metrics
var vnetLogs = [
  {
    category: 'VMProtectionAlerts'
    enabled: true
  }
]
var vnetMetrics = [
  {
    category: 'AllMetrics'
    enabled: true
  }
]

// Bastion logs and metrics
var bastionLogs = [
  {
    category: 'BastionAuditLogs'
    enabled: true
  }
]
var bastionMetrics = [
  {
    category: 'AllMetrics'
    enabled: true
  }
]

// Public IP logs and metrics
var publicIpLogs = [
  {
    category: 'DDoSProtectionNotifications'
    enabled: true
  }
  {
    category: 'DDoSMitigationFlowLogs'
    enabled: true
  }
  {
    category: 'DDoSMitigationReports'
    enabled: true
  }
]
var publicIpMetrics = [
  {
    category: 'AllMetrics'
    enabled: true
  }
]

// Reference existing resources based on type
resource nsgResource 'Microsoft.Network/networkSecurityGroups@2024-03-01' existing = if (resourceType == 'nsg') {
  name: targetResourceName
}

resource vnetResource 'Microsoft.Network/virtualNetworks@2024-03-01' existing = if (resourceType == 'vnet') {
  name: targetResourceName
}

resource bastionResource 'Microsoft.Network/bastionHosts@2024-03-01' existing = if (resourceType == 'bastion') {
  name: targetResourceName
}

resource publicIpResource 'Microsoft.Network/publicIPAddresses@2024-03-01' existing = if (resourceType == 'publicip') {
  name: targetResourceName
}

// Diagnostic Settings for NSG
resource nsgDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (resourceType == 'nsg') {
  name: diagnosticSettingsName
  scope: nsgResource
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: nsgLogs
  }
}

// Diagnostic Settings for VNet
resource vnetDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (resourceType == 'vnet') {
  name: diagnosticSettingsName
  scope: vnetResource
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: vnetLogs
    metrics: vnetMetrics
  }
}

// Diagnostic Settings for Bastion
resource bastionDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (resourceType == 'bastion') {
  name: diagnosticSettingsName
  scope: bastionResource
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: bastionLogs
    metrics: bastionMetrics
  }
}

// Diagnostic Settings for Public IP
resource publicIpDiagnosticSettings 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = if (resourceType == 'publicip') {
  name: diagnosticSettingsName
  scope: publicIpResource
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: publicIpLogs
    metrics: publicIpMetrics
  }
}

// Outputs
output diagnosticSettingsName string = diagnosticSettingsName
