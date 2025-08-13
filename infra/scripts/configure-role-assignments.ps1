#Requires -Module Az.Accounts, Az.Resources, Az.ManagedServiceIdentity, Az.Storage, Az.KeyVault, Az.ServiceBus, Az.CognitiveServices, Az.EventGrid

<#
.SYNOPSIS
    Configures role assignments for AI-One solution managed identities.

.DESCRIPTION
    This script automates the configuration of role assignments for the AI-One solution.
    It assigns the necessary roles to the API App's managed identity and Event Grid system topic
    to enable proper service-to-service authentication.

.PARAMETER SubscriptionId
    The Azure subscription ID where resources are deployed.

.PARAMETER ResourceGroupName
    The name of the resource group containing the AI-One resources.

.PARAMETER ApiAppManagedIdentityName
    The name of the API App's user-assigned managed identity.

.PARAMETER OpenAIServiceName
    The name of the Azure OpenAI service.

.PARAMETER StorageAccountName
    The name of the storage account.

.PARAMETER DocumentIntelligenceServiceName
    The name of the Document Intelligence service.

.PARAMETER ServiceBusName
    The name of the Service Bus namespace.

.PARAMETER KeyVaultName
    The name of the Key Vault.

.PARAMETER EventGridTopicName
    The name of the Event Grid system topic.

.PARAMETER AiFoundryAccountName
    The name of the AI Foundry account.

.PARAMETER AiFoundryProjectName
    The name of the AI Foundry project.

.EXAMPLE
    .\configure-role-assignments.ps1 -SubscriptionId "12345678-1234-1234-1234-123456789012" -ResourceGroupName "rg-practice-ai-aione-dev" -ApiAppManagedIdentityName "id-ag-aione-dev-apiapp" -OpenAIServiceName "ag-aione-dev-foundry" -StorageAccountName "agaionedevsto" -DocumentIntelligenceServiceName "ag-aione-dev-docintel" -ServiceBusName "ag-aione-dev-service-bus" -KeyVaultName "ag-aione-dev-kv" -EventGridTopicName "ag-aione-dev-blob-eg" -AiFoundryAccountName "ag-aione-dev-foundry" -AiFoundryProjectName "ag-aione-dev-prj"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory = $true)]
    [string]$ApiAppManagedIdentityName,
    
    [Parameter(Mandatory = $true)]
    [string]$OpenAIServiceName,
    
    [Parameter(Mandatory = $true)]
    [string]$StorageAccountName,
    
    [Parameter(Mandatory = $true)]
    [string]$DocumentIntelligenceServiceName,
    
    [Parameter(Mandatory = $true)]
    [string]$ServiceBusName,
    
    [Parameter(Mandatory = $true)]
    [string]$KeyVaultName,
    
    [Parameter(Mandatory = $true)]
    [string]$EventGridTopicName,
    
    [Parameter(Mandatory = $true)]
    [string]$AiFoundryAccountName,
    
    [Parameter(Mandatory = $true)]
    [string]$AiFoundryProjectName
)

# Error handling
$ErrorActionPreference = "Stop"

# Role definitions
$RoleDefinitions = @{
    "CognitiveServicesOpenAIUser" = "5e0bd9bd-7b93-4f28-af87-19fc36ad61bd"
    "StorageBlobDataContributor" = "ba92f5b4-2d11-453d-a403-e96b0029c9fe"
    "CognitiveServicesUser" = "a97b65f3-24c7-4388-baec-2e87135dc908"
    "ServiceBusDataReceiver" = "4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0"
    "ServiceBusDataSender" = "69a216fc-b8fb-44d8-bc22-1f3c2cd27a39"
    "KeyVaultSecretsUser" = "4633458b-17de-408a-b874-0445c86b69e6"
    "AzureAIUser" = "53ca6127-db72-4b80-b1b0-d745d6d5456d"
}

# Initialize resource names - all names are now provided as parameters
$ResourceNames = @{
    "ApiAppManagedIdentity" = $ApiAppManagedIdentityName
    "OpenAIService" = $OpenAIServiceName
    "StorageAccount" = $StorageAccountName
    "DocumentIntelligence" = $DocumentIntelligenceServiceName
    "ServiceBus" = $ServiceBusName
    "KeyVault" = $KeyVaultName
    "EventGridTopic" = $EventGridTopicName
}

function Write-StatusMessage {
    param(
        [string]$Message,
        [string]$Type = "Info"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch ($Type) {
        "Success" { "Green" }
        "Warning" { "Yellow" }
        "Error" { "Red" }
        default { "White" }
    }
    
    Write-Host "[$timestamp] $Message" -ForegroundColor $color
}

function Test-AzureConnection {
    try {
        $context = Get-AzContext
        if ($null -eq $context) {
            Write-StatusMessage "Not connected to Azure. Please run Connect-AzAccount first." -Type "Error"
            exit 1
        }
        
        if ($context.Subscription.Id -ne $SubscriptionId) {
            Write-StatusMessage "Setting Azure context to subscription: $SubscriptionId" -Type "Info"
            Set-AzContext -SubscriptionId $SubscriptionId | Out-Null
        }
        
        Write-StatusMessage "Connected to Azure subscription: $($context.Subscription.Name)" -Type "Success"
    }
    catch {
        Write-StatusMessage "Failed to connect to Azure: $($_.Exception.Message)" -Type "Error"
        exit 1
    }
}

function Get-ResourceByName {
    param(
        [string]$ResourceName,
        [string]$ResourceType
    )
    
    try {
        $resource = Get-AzResource -ResourceGroupName $ResourceGroupName -Name $ResourceName -ResourceType $ResourceType -ErrorAction SilentlyContinue
        if ($null -eq $resource) {
            Write-StatusMessage "Resource not found: $ResourceName ($ResourceType)" -Type "Warning"
            return $null
        }
        return $resource
    }
    catch {
        Write-StatusMessage "Error finding resource $ResourceName`: $($_.Exception.Message)" -Type "Error"
        return $null
    }
}

function New-RoleAssignmentSafe {
    param(
        [string]$ObjectId,
        [string]$RoleDefinitionId,
        [string]$Scope,
        [string]$DisplayName
    )
    
    try {
        # Check if role assignment already exists
        $existingAssignment = Get-AzRoleAssignment -ObjectId $ObjectId -RoleDefinitionId $RoleDefinitionId -Scope $Scope -ErrorAction SilentlyContinue
        
        if ($existingAssignment) {
            Write-StatusMessage "Role assignment already exists: $DisplayName" -Type "Warning"
            return $true
        }
        
        $roleAssignment = New-AzRoleAssignment -ObjectId $ObjectId -RoleDefinitionId $RoleDefinitionId -Scope $Scope
        Write-StatusMessage "Successfully created role assignment: $DisplayName" -Type "Success"
        return $true
    }
    catch {
        Write-StatusMessage "Failed to create role assignment $DisplayName`: $($_.Exception.Message)" -Type "Error"
        return $false
    }
}

function Configure-ApiAppRoleAssignments {
    param(
        [object]$ManagedIdentity
    )
    
    Write-StatusMessage "Configuring role assignments for API App managed identity..." -Type "Info"
    
    $assignments = @()
    
    # Azure OpenAI Service
    $openAiResource = Get-ResourceByName -ResourceName $ResourceNames.OpenAIService -ResourceType "Microsoft.CognitiveServices/accounts"
    if ($openAiResource) {
        $assignments += @{
            Resource = $openAiResource
            RoleId = $RoleDefinitions.CognitiveServicesOpenAIUser
            DisplayName = "Cognitive Services OpenAI User on $($openAiResource.Name)"
        }
    }
    
    # Storage Account
    $storageResource = Get-ResourceByName -ResourceName $ResourceNames.StorageAccount -ResourceType "Microsoft.Storage/storageAccounts"
    if ($storageResource) {
        $assignments += @{
            Resource = $storageResource
            RoleId = $RoleDefinitions.StorageBlobDataContributor
            DisplayName = "Storage Blob Data Contributor on $($storageResource.Name)"
        }
    }
    
    # Document Intelligence
    $docIntelResource = Get-ResourceByName -ResourceName $ResourceNames.DocumentIntelligence -ResourceType "Microsoft.CognitiveServices/accounts"
    if ($docIntelResource) {
        $assignments += @{
            Resource = $docIntelResource
            RoleId = $RoleDefinitions.CognitiveServicesUser
            DisplayName = "Cognitive Services User on $($docIntelResource.Name)"
        }
    }
    
    # Service Bus (Receiver and Sender)
    $serviceBusResource = Get-ResourceByName -ResourceName $ResourceNames.ServiceBus -ResourceType "Microsoft.ServiceBus/namespaces"
    if ($serviceBusResource) {
        $assignments += @{
            Resource = $serviceBusResource
            RoleId = $RoleDefinitions.ServiceBusDataReceiver
            DisplayName = "Azure Service Bus Data Receiver on $($serviceBusResource.Name)"
        }
        $assignments += @{
            Resource = $serviceBusResource
            RoleId = $RoleDefinitions.ServiceBusDataSender
            DisplayName = "Azure Service Bus Data Sender on $($serviceBusResource.Name)"
        }
    }
    
    # Key Vault
    $keyVaultResource = Get-ResourceByName -ResourceName $ResourceNames.KeyVault -ResourceType "Microsoft.KeyVault/vaults"
    if ($keyVaultResource) {
        $assignments += @{
            Resource = $keyVaultResource
            RoleId = $RoleDefinitions.KeyVaultSecretsUser
            DisplayName = "Key Vault Secrets User on $($keyVaultResource.Name)"
        }
    }
    
    # AI Foundry Project
    Write-StatusMessage "Configuring AI Foundry project role assignment..." -Type "Info"
    
    # Get AI Foundry account first
    $aiFoundryAccount = Get-ResourceByName -ResourceName $AiFoundryAccountName -ResourceType "Microsoft.CognitiveServices/accounts"
    if ($aiFoundryAccount) {
        # Get AI Foundry project (nested resource)
        try {
            $aiFoundryProjectId = "$($aiFoundryAccount.ResourceId)/aiservices/$AiFoundryProjectName"
            Write-StatusMessage "Adding Azure AI User role for AI Foundry project: $AiFoundryProjectName" -Type "Info"
            
            $assignments += @{
                Resource = @{ ResourceId = $aiFoundryProjectId; Name = $AiFoundryProjectName }
                RoleId = $RoleDefinitions.AzureAIUser
                DisplayName = "Azure AI User on AI Foundry project $AiFoundryProjectName"
            }
        }
        catch {
            Write-StatusMessage "Error configuring AI Foundry project role: $($_.Exception.Message)" -Type "Warning"
        }
    }
    else {
        Write-StatusMessage "AI Foundry account '$AiFoundryAccountName' not found. Skipping AI Foundry role assignment." -Type "Warning"
    }
    
    # Execute role assignments
    $successCount = 0
    foreach ($assignment in $assignments) {
        $success = New-RoleAssignmentSafe -ObjectId $ManagedIdentity.PrincipalId -RoleDefinitionId $assignment.RoleId -Scope $assignment.Resource.ResourceId -DisplayName $assignment.DisplayName
        if ($success) { $successCount++ }
    }
    
    Write-StatusMessage "API App role assignments completed: $successCount/$($assignments.Count) successful" -Type "Info"
    return $successCount -eq $assignments.Count
}

function Configure-EventGridRoleAssignments {
    Write-StatusMessage "Configuring role assignments for Event Grid system topic..." -Type "Info"
    
    # Get Event Grid system topic
    $eventGridResource = Get-ResourceByName -ResourceName $ResourceNames.EventGridTopic -ResourceType "Microsoft.EventGrid/systemTopics"
    if (-not $eventGridResource) {
        Write-StatusMessage "Event Grid system topic not found. Skipping Event Grid role assignments." -Type "Warning"
        return $false
    }
    
    # Get system-assigned managed identity for Event Grid
    try {
        $eventGridTopic = Get-AzEventGridTopic -ResourceGroupName $ResourceGroupName -Name $ResourceNames.EventGridTopic -ErrorAction SilentlyContinue
        if (-not $eventGridTopic -or -not $eventGridTopic.Identity -or -not $eventGridTopic.Identity.PrincipalId) {
            Write-StatusMessage "Event Grid system topic does not have system-assigned managed identity enabled." -Type "Warning"
            return $false
        }
        
        $eventGridPrincipalId = $eventGridTopic.Identity.PrincipalId
        Write-StatusMessage "Found Event Grid system topic with managed identity: $eventGridPrincipalId" -Type "Success"
        
    }
    catch {
        Write-StatusMessage "Failed to get Event Grid system topic identity: $($_.Exception.Message)" -Type "Error"
        return $false
    }
    
    # Assign Service Bus Data Sender role to Event Grid
    $serviceBusResource = Get-ResourceByName -ResourceName $ResourceNames.ServiceBus -ResourceType "Microsoft.ServiceBus/namespaces"
    if ($serviceBusResource) {
        $success = New-RoleAssignmentSafe -ObjectId $eventGridPrincipalId -RoleDefinitionId $RoleDefinitions.ServiceBusDataSender -Scope $serviceBusResource.ResourceId -DisplayName "Azure Service Bus Data Sender on $($serviceBusResource.Name) for Event Grid"
        
        if ($success) {
            Write-StatusMessage "Event Grid role assignments completed successfully" -Type "Success"
            return $true
        }
    }
    
    return $false
}

function Main {
    Write-StatusMessage "Starting AI-One role assignment configuration..." -Type "Info"
    Write-StatusMessage "Subscription: $SubscriptionId" -Type "Info"
    Write-StatusMessage "Resource Group: $ResourceGroupName" -Type "Info"
    
    # Display resolved resource names
    Write-StatusMessage "Resource names to configure:" -Type "Info"
    Write-StatusMessage "  API App Managed Identity: $($ResourceNames.ApiAppManagedIdentity)" -Type "Info"
    Write-StatusMessage "  OpenAI Service: $($ResourceNames.OpenAIService)" -Type "Info"
    Write-StatusMessage "  Storage Account: $($ResourceNames.StorageAccount)" -Type "Info"
    Write-StatusMessage "  Document Intelligence: $($ResourceNames.DocumentIntelligence)" -Type "Info"
    Write-StatusMessage "  Service Bus: $($ResourceNames.ServiceBus)" -Type "Info"
    Write-StatusMessage "  Key Vault: $($ResourceNames.KeyVault)" -Type "Info"
    Write-StatusMessage "  Event Grid Topic: $($ResourceNames.EventGridTopic)" -Type "Info"
    
    # Test Azure connection
    Test-AzureConnection
    
    # Verify resource group exists
    try {
        $resourceGroup = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction Stop
        Write-StatusMessage "Found resource group: $($resourceGroup.ResourceGroupName)" -Type "Success"
    }
    catch {
        Write-StatusMessage "Resource group not found: $ResourceGroupName" -Type "Error"
        exit 1
    }
    
    # Get API App managed identity
    Write-StatusMessage "Looking for API App managed identity: $($ResourceNames.ApiAppManagedIdentity)" -Type "Info"
    
    try {
        $managedIdentity = Get-AzUserAssignedIdentity -ResourceGroupName $ResourceGroupName -Name $ResourceNames.ApiAppManagedIdentity -ErrorAction Stop
        Write-StatusMessage "Found API App managed identity: $($managedIdentity.Name) (Principal ID: $($managedIdentity.PrincipalId))" -Type "Success"
    }
    catch {
        Write-StatusMessage "API App managed identity not found: $($ResourceNames.ApiAppManagedIdentity)" -Type "Error"
        Write-StatusMessage "Error: $($_.Exception.Message)" -Type "Error"
        exit 1
    }
    
    # Configure role assignments
    $apiAppSuccess = Configure-ApiAppRoleAssignments -ManagedIdentity $managedIdentity
    $eventGridSuccess = Configure-EventGridRoleAssignments
    
    # Summary
    Write-StatusMessage "Role assignment configuration completed!" -Type "Info"
    Write-StatusMessage "API App role assignments: $(if ($apiAppSuccess) { 'SUCCESS' } else { 'FAILED' })" -Type $(if ($apiAppSuccess) { "Success" } else { "Error" })
    Write-StatusMessage "Event Grid role assignments: $(if ($eventGridSuccess) { 'SUCCESS' } else { 'WARNING' })" -Type $(if ($eventGridSuccess) { "Success" } else { "Warning" })
    
    if ($apiAppSuccess) {
        Write-StatusMessage "All critical role assignments have been configured successfully!" -Type "Success"
        Write-StatusMessage "Your AI-One application should now have the necessary permissions for service-to-service authentication." -Type "Success"
    }
    else {
        Write-StatusMessage "Some role assignments failed. Please check the errors above and retry." -Type "Error"
        exit 1
    }
}

# Execute main function
Main
