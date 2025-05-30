@description('Azure region for resource deployment')
param location string

@description('Service Bus namespace name')
param serviceBusName string

@description('Service Bus queue name')
param serviceBusQueueName string

resource serviceBus 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  location: location
  name: serviceBusName

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

output serviceBusName string = serviceBus.name
output serviceBusId string = serviceBus.id
output serviceBusQueueName string = serviceBus::queue.name
output serviceBusQueueId string = serviceBus::queue.id
