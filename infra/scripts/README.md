# Certificate Upload Guide

This directory contains scripts to help you upload SSL certificates to Azure Key Vault for use with Application Gateway.

## Scripts Available

### 1. `upload-certificate.ps1`
Uploads an existing PFX certificate to Azure Key Vault.

### 2. `create-and-upload-cert.ps1`
Creates a self-signed certificate and uploads it to Azure Key Vault.

## Usage Examples

### Upload Existing Commercial Certificate

```powershell
# Basic usage with required parameters
.\upload-certificate.ps1 `
  -CertificateFilePath "C:\path\to\your\certificate.pfx" `
  -CertificatePassword "YourCertificatePassword"

# Full usage with all parameters
.\upload-certificate.ps1 `
  -CertificateFilePath "C:\path\to\your\certificate.pfx" `
  -CertificatePassword "YourCertificatePassword" `
  -KeyVaultName "ag-aionev14-uat-kv" `
  -CertificateName "agile-chat-ssl-cert" `
  -Force
```

### Create and Upload Self-Signed Certificate

```powershell
# Create self-signed certificate for development/testing
.\create-and-upload-cert.ps1 `
  -DomainName "your-domain.com" `
  -KeyVaultName "ag-aionev14-uat-kv" `
  -CertificateName "agile-chat-ssl-cert"
```

## Prerequisites

1. **Azure CLI**: Install and login with `az login`
2. **PowerShell**: Windows PowerShell 5.1 or PowerShell Core 7+
3. **Key Vault Access**: Ensure you have Certificate Officer permissions on the Key Vault
4. **Certificate File**: PFX format certificate file for upload

## Parameters

### upload-certificate.ps1

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `CertificateFilePath` | Yes | - | Path to your PFX certificate file |
| `CertificatePassword` | Yes | - | Password for the PFX certificate |
| `KeyVaultName` | No | `ag-aionev14-uat-kv` | Azure Key Vault name |
| `CertificateName` | No | `agile-chat-ssl-cert` | Name for the certificate in Key Vault |
| `Force` | No | `false` | Overwrite existing certificate without prompting |

## After Upload

1. Update your `uat.bicepparam` file:
   ```bicep
   param sslCertificateSecretName = 'agile-chat-ssl-cert'
   param enableHttps = true
   ```

2. Deploy your Application Gateway:
   ```powershell
   az deployment group create \
     --resource-group <your-resource-group> \
     --template-file infra/platform/appgw/main.bicep \
     --parameters infra/platform/appgw/uat.bicepparam
   ```

## Troubleshooting

- **Access Denied**: Ensure you have Key Vault Certificate Officer role
- **Certificate Format**: Only PFX/P12 formats are supported
- **Password Issues**: Verify the certificate password is correct
- **Key Vault Not Found**: Check Key Vault name and resource group
