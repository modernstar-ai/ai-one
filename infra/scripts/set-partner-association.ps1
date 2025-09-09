[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId,
    
    [Parameter(Mandatory = $false)]
    [string]$MpnId = "5130142" 
)

Write-Host "=== Linking Partner MPN ID ==="
Write-Host "MPN ID: $MpnId"
Write-Host "Subscription: $SubscriptionId"

# Set context to subscription
az account set --subscription $SubscriptionId

try {
    $currentPartner = az managementpartner show 2>$null | ConvertFrom-Json
    if ($currentPartner) {
        Write-Host "Current partner: $($currentPartner.partnerId) - $($currentPartner.partnerName)"
        if ($currentPartner.partnerId -ne $MpnId) {
            Write-Host "Updating partner ID from $($currentPartner.partnerId) to $MpnId"
            az managementpartner update --partner-id $MpnId
        } else {
            Write-Host "✅ Correct partner ID already linked"
        }
    }
} catch {
    Write-Host "No existing partner link found, creating new association..."
    az managementpartner create --partner-id $MpnId
}

$verifyPartner = az managementpartner show | ConvertFrom-Json
if ($verifyPartner.partnerId -eq $MpnId) {
    Write-Host "✅ Successfully linked MPN ID: $MpnId"
    Write-Host "Partner Name: $($verifyPartner.partnerName)"
    Write-Host "State: $($verifyPartner.state)"
} else {
    Write-Error "❌ Failed to link MPN ID. Expected: $MpnId, Got: $($verifyPartner.partnerId)"
    exit 1
}
