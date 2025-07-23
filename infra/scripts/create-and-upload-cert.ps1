# PowerShell script to create and upload SSL certificate for Application Gateway
# This creates a self-signed certificate and uploads it to Azure Key Vault

param(
    [Parameter(Mandatory = $true)]
    [string]$DomainName,
    
    [Parameter(Mandatory = $true)]
    [string]$KeyVaultName,
    
    [Parameter(Mandatory = $true)]
    [string]$CertificateName,
    
    [Parameter(Mandatory = $false)]
    [string]$CertificatePassword = "P@ssw0rd123!",
    
    [Parameter(Mandatory = $false)]
    [int]$ValidityDays = 365
)

Write-Host "Creating SSL certificate for domain: $DomainName" -ForegroundColor Green

try {
    # Create self-signed certificate
    $cert = New-SelfSignedCertificate `
        -DnsName $DomainName `
        -CertStoreLocation "cert:\LocalMachine\My" `
        -NotAfter (Get-Date).AddDays($ValidityDays) `
        -KeyLength 2048 `
        -KeyAlgorithm RSA `
        -HashAlgorithm SHA256

    Write-Host "Certificate created with thumbprint: $($cert.Thumbprint)" -ForegroundColor Yellow

    # Export certificate to PFX file
    $pfxPath = "$env:TEMP\$CertificateName.pfx"
    $securePassword = ConvertTo-SecureString $CertificatePassword -AsPlainText -Force
    
    Export-PfxCertificate `
        -Cert $cert `
        -FilePath $pfxPath `
        -Password $securePassword `
        -Force

    Write-Host "Certificate exported to: $pfxPath" -ForegroundColor Yellow

    # Upload to Azure Key Vault
    Write-Host "Uploading certificate to Key Vault: $KeyVaultName" -ForegroundColor Green
    
    Import-AzKeyVaultCertificate `
        -VaultName $KeyVaultName `
        -Name $CertificateName `
        -FilePath $pfxPath `
        -Password $securePassword

    Write-Host "Certificate successfully uploaded to Key Vault!" -ForegroundColor Green

    # Clean up local certificate and file
    Remove-Item "cert:\LocalMachine\My\$($cert.Thumbprint)" -Force
    Remove-Item $pfxPath -Force

    Write-Host "Local certificate and file cleaned up." -ForegroundColor Yellow
    Write-Host "Certificate '$CertificateName' is now available in Key Vault '$KeyVaultName'" -ForegroundColor Green

} catch {
    Write-Error "Failed to create or upload certificate: $($_.Exception.Message)"
    exit 1
}
