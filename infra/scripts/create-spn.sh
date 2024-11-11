#!/bin/bash

# Variables
SUBSCRIPTION_ID="<SUBID>"
SP_NAME="github-actions-tst"  # Change to a meaningful name
ROLE="Contributor"           # Change role as needed (e.g., "Contributor" or "Reader")
SCOPE="/subscriptions/$SUBSCRIPTION_ID"  # Scope can be set to specific resource group or resource

# Create Service Principal
echo "Creating service principal '$SP_NAME'..."
SP_CREDENTIALS=$(az ad sp create-for-rbac \
    --name "$SP_NAME" \
    --role "$ROLE" \
    --scopes "$SCOPE" \
    --query "{client_id:appId, client_secret:password, tenant_id:tenant}" \
    -o json)

if [[ -z "$SP_CREDENTIALS" ]]; then
    echo "Failed to create service principal."
    exit 1
fi

# Output JSON credentials
CLIENT_ID=$(echo "$SP_CREDENTIALS" | jq -r '.client_id')
CLIENT_SECRET=$(echo "$SP_CREDENTIALS" | jq -r '.client_secret')
TENANT_ID=$(echo "$SP_CREDENTIALS" | jq -r '.tenant_id')

echo "Service principal created successfully."
echo ""
echo "GitHub Actions Secret Values:"
echo "-----------------------------"
echo "AZURE_CLIENT_ID: $CLIENT_ID"
echo "AZURE_CLIENT_SECRET: $CLIENT_SECRET"
echo "AZURE_TENANT_ID: $TENANT_ID"
echo "AZURE_SUBSCRIPTION_ID: $SUBSCRIPTION_ID"
echo ""
echo "Add these as secrets in your GitHub repository's settings for use in GitHub Actions."