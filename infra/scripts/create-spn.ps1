param (
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId,

    [Parameter(Mandatory = $false)]
    [string]$SpName = "github-actions-sp"
)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# Ensure Az module is installed
if (-not (Get-Module -ListAvailable -Name Az)) {
    Install-Module -Name Az -AllowClobber -Scope CurrentUser -Repository PSGallery -Force
}

##Connect-AzAccount

# Variables
#$SubscriptionId = ""
#$SpName = "github-actions-sp"  # Change to a meaningful name
$Role = "Contributor"          # Change role as needed (e.g., "Contributor" or "Reader")
$Scope = "/subscriptions/$SubscriptionId"  # Scope can be set to specific resource group or resource

# Set the subscription context
Set-AzContext -SubscriptionId $SubscriptionId

# Create Service Principal
Write-Output "Creating service principal '$SpName'..."
$sp = New-AzADServicePrincipal -DisplayName $SpName

if (-not $sp) {
    Write-Output "Failed to create service principal."
    exit 1
}

# Assign role to the Service Principal
New-AzRoleAssignment -ObjectId $sp.Id -RoleDefinitionName $Role -Scope $Scope

# Output JSON credentials
$ClientId = $sp.ApplicationId
$ClientSecret = ($sp | Get-AzADAppCredential).SecretText
$TenantId = (Get-AzContext).Tenant.Id

Write-Output "Service principal created successfully."
Write-Output ""
Write-Output "GitHub Actions Secret Values:"
Write-Output "-----------------------------"
Write-Output "AZURE_CLIENT_ID: $ClientId"
Write-Output "AZURE_CLIENT_SECRET: $ClientSecret"
Write-Output "AZURE_TENANT_ID: $TenantId"
Write-Output "AZURE_SUBSCRIPTION_ID: $SubscriptionId"
Write-Output ""
Write-Output "Add these as secrets in your GitHub repository's settings for use in GitHub Actions."
