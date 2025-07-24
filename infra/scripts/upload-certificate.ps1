# PowerShell script to upload an existing PFX certificate to Azure Key Vault
# This script uploads a commercial or existing SSL certificate to Key Vault for Application Gateway use

param(
    [Parameter(Mandatory = $true)]
    [string]$CertificateFilePath,
    
    [Parameter(Mandatory = $true)]
    [string]$CertificatePassword,
    
    [Parameter(Mandatory = $false)]
    [string]$KeyVaultName = "ag-aionev14-uat-kv",
    
    [Parameter(Mandatory = $false)]
    [string]$CertificateName = "agile-chat-ssl-cert",
    
    [Parameter(Mandatory = $false)]
    [switch]$Force
)

Write-Host "=== Azure Key Vault Certificate Upload Script ===" -ForegroundColor Cyan
Write-Host "Key Vault: $KeyVaultName" -ForegroundColor Yellow
Write-Host "Certificate Name: $CertificateName" -ForegroundColor Yellow
Write-Host "Certificate File: $CertificateFilePath" -ForegroundColor Yellow

# Validate inputs
if (-not (Test-Path $CertificateFilePath)) {
    Write-Error "Certificate file not found: $CertificateFilePath"
    exit 1
}

if ($CertificateFilePath -notmatch '\.(pfx|p12)$') {
    Write-Warning "File extension should be .pfx or .p12 for PKCS#12 format"
}

try {
    # Check if Azure CLI is installed and user is logged in
    Write-Host "Checking Azure CLI authentication..." -ForegroundColor Green
    $azAccount = az account show 2>$null | ConvertFrom-Json
    
    if (-not $azAccount) {
        Write-Error "Not logged in to Azure CLI. Please run 'az login' first."
        exit 1
    }
    
    Write-Host "Logged in as: $($azAccount.user.name)" -ForegroundColor Green
    Write-Host "Subscription: $($azAccount.name) ($($azAccount.id))" -ForegroundColor Green

    # Check if Key Vault exists
    Write-Host "Verifying Key Vault access..." -ForegroundColor Green
    $kvExists = az keyvault show --name $KeyVaultName 2>$null
    
    if (-not $kvExists) {
        Write-Error "Key Vault '$KeyVaultName' not found or no access. Please check the name and permissions."
        exit 1
    }

    # Check if certificate already exists
    $existingCert = az keyvault certificate show --vault-name $KeyVaultName --name $CertificateName 2>$null
    
    if ($existingCert -and -not $Force) {
        Write-Warning "Certificate '$CertificateName' already exists in Key Vault '$KeyVaultName'"
        $response = Read-Host "Do you want to overwrite it? (y/N)"
        if ($response -ne 'y' -and $response -ne 'Y') {
            Write-Host "Operation cancelled." -ForegroundColor Yellow
            exit 0
        }
    }

    # Upload certificate to Key Vault
    Write-Host "Uploading certificate to Key Vault..." -ForegroundColor Green
    
    $result = az keyvault certificate import `
        --vault-name $KeyVaultName `
        --name $CertificateName `
        --file $CertificateFilePath `
        --password $CertificatePassword `
        --output json 2>&1

    if ($LASTEXITCODE -eq 0) {
        $certInfo = $result | ConvertFrom-Json
        Write-Host "✅ Certificate uploaded successfully!" -ForegroundColor Green
        Write-Host "Certificate ID: $($certInfo.id)" -ForegroundColor Yellow
        Write-Host "Thumbprint: $($certInfo.x509Thumbprint)" -ForegroundColor Yellow
        Write-Host "Valid From: $($certInfo.attributes.notBefore)" -ForegroundColor Yellow
        Write-Host "Valid To: $($certInfo.attributes.expires)" -ForegroundColor Yellow
        
        # Display next steps
        Write-Host "`n=== Next Steps ===" -ForegroundColor Cyan
        Write-Host "1. Your certificate is now available in Key Vault as: '$CertificateName'" -ForegroundColor White
        Write-Host "2. Update your Bicep parameters file with:" -ForegroundColor White
        Write-Host "   param sslCertificateSecretName = '$CertificateName'" -ForegroundColor Gray
        Write-Host "   param enableHttps = true" -ForegroundColor Gray
        Write-Host "3. Deploy your Application Gateway infrastructure" -ForegroundColor White
        Write-Host "4. The Application Gateway will automatically use this certificate via managed identity" -ForegroundColor White
        
    } else {
        Write-Error "Failed to upload certificate: $result"
        exit 1
    }

} catch {
    Write-Error "An error occurred: $($_.Exception.Message)"
    exit 1
}

Write-Host "`n=== Upload Complete ===" -ForegroundColor Green
