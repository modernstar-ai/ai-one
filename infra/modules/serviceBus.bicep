@description('Name of the Service Bus')
param name string

@description('Specifies the location for all the Azure resources.')
param location string

@description('Optional. Tags to be applied to the resources.')
param tags object = {}

@description('Service Bus queue name')
param serviceBusQueueName string

resource serviceBus 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  location: location
  name: name
  tags: tags
  resource queue 'queues' = {
    name: serviceBusQueueName
    properties: {
      maxMessageSizeInKilobytes: 256
      lockDuration: 'PT5M'
      maxSizeInMegabytes: 5120
      requiresDuplicateDetection: false
      requiresSession: false
      defaultMessageTimeToLive: 'P14D'
      deadLetteringOnMessageExpiration: true
      enableBatchedOperations: true
      duplicateDetectionHistoryTimeWindow: 'PT10M'
      maxDeliveryCount: 5
      status: 'Active'
      autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
      enablePartitioning: false
      enableExpress: false
    }
  }
}

output name string = serviceBus.name
output resourceId string = serviceBus.id
output serviceBusQueueName string = serviceBus::queue.name
output serviceBusQueueResourceId string = serviceBus::queue.id
