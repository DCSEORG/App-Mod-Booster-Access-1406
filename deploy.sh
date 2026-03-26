#!/bin/bash
set -e

# ============================================================
# Northwind App - Azure Deployment Script
# Usage: bash deploy.sh
# ============================================================

RESOURCE_GROUP="${RESOURCE_GROUP:-rg-northwind-app}"
LOCATION="${LOCATION:-uksouth}"
RESOURCE_PREFIX="${RESOURCE_PREFIX:-northwind}"
ADMIN_OBJECT_ID="${ADMIN_OBJECT_ID:-$(az ad signed-in-user show --query id -o tsv)}"
ADMIN_LOGIN="${ADMIN_LOGIN:-$(az ad signed-in-user show --query userPrincipalName -o tsv)}"

echo "============================================================"
echo " Northwind App - Azure Deployment"
echo "============================================================"
echo " Resource Group : $RESOURCE_GROUP"
echo " Location       : $LOCATION"
echo " Resource Prefix: $RESOURCE_PREFIX"
echo "============================================================"

# 1. Create resource group
echo ""
echo "Step 1: Creating resource group..."
az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --output none
echo "  ✓ Resource group ready"

# 2. Deploy Bicep (main.bicep)
echo ""
echo "Step 2: Deploying Azure infrastructure (Bicep)..."
DEPLOYMENT_OUTPUT=$(az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --name main \
  --template-file ./infra/main.bicep \
  --parameters \
    location="$LOCATION" \
    resourcePrefix="$RESOURCE_PREFIX" \
    adminObjectId="$ADMIN_OBJECT_ID" \
    adminLogin="$ADMIN_LOGIN" \
    deployGenAI=false \
  --query "properties.outputs" \
  --output json)

echo "  ✓ Infrastructure deployed"

# 3. Read outputs
echo ""
echo "Step 3: Reading deployment outputs..."
APP_SERVICE_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServiceName.value')
SQL_SERVER_FQDN=$(echo $DEPLOYMENT_OUTPUT | jq -r '.sqlServerFqdn.value')
MANAGED_IDENTITY_CLIENT_ID=$(echo $DEPLOYMENT_OUTPUT | jq -r '.managedIdentityClientId.value')
MANAGED_IDENTITY_PRINCIPAL_ID=$(echo $DEPLOYMENT_OUTPUT | jq -r '.managedIdentityPrincipalId.value')
MANAGED_IDENTITY_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.managedIdentityName.value')
APP_HOSTNAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.appServiceHostname.value')

echo "  App Service  : $APP_SERVICE_NAME"
echo "  SQL FQDN     : $SQL_SERVER_FQDN"
echo "  MI Client ID : $MANAGED_IDENTITY_CLIENT_ID"
echo "  App URL      : https://$APP_HOSTNAME"

# 4. Set App Service settings
echo ""
echo "Step 4: Configuring App Service settings..."
CONNECTION_STRING="Server=tcp:${SQL_SERVER_FQDN},1433;Database=Northwind;Authentication=Active Directory Managed Identity;User Id=${MANAGED_IDENTITY_CLIENT_ID};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az webapp config appsettings set \
  --name "$APP_SERVICE_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --settings \
    "ConnectionStrings__DefaultConnection=${CONNECTION_STRING}" \
    "AZURE_CLIENT_ID=${MANAGED_IDENTITY_CLIENT_ID}" \
    "ManagedIdentityClientId=${MANAGED_IDENTITY_CLIENT_ID}" \
  --output none

echo "  ✓ App Service settings configured"

# 5. Wait for SQL
echo ""
echo "Step 5: Waiting 30 seconds for SQL Server to be ready..."
sleep 30

# 6. Add firewall rules
echo ""
echo "Step 6: Adding firewall rules..."
echo "Adding current IP to SQL firewall..."
MY_IP=$(curl -s https://api.ipify.org)
SQL_SERVER_NAME=$(echo $SQL_SERVER_FQDN | cut -d'.' -f1)
az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "AllowAllAzureIPs" \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0 \
  --output none
az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "AllowDeploymentIP" \
  --start-ip-address "$MY_IP" \
  --end-ip-address "$MY_IP" \
  --output none
echo "  ✓ Firewall rules added (Azure services + $MY_IP)"

# 7. Wait for firewall propagation
echo ""
echo "Step 7: Waiting additional 15 seconds for firewall rules to propagate..."
sleep 15

# 8. Install Python dependencies
echo ""
echo "Step 8: Installing Python dependencies..."
pip3 install --quiet pyodbc azure-identity
echo "  ✓ Python dependencies installed"

# 9. Import database schema
echo ""
echo "Step 9: Importing database schema..."
SQL_SERVER_FQDN="$SQL_SERVER_FQDN" python3 run-sql.py
echo "  ✓ Database schema imported"

# 10. Set database roles for managed identity
echo ""
echo "Step 10: Configuring database roles for managed identity..."
# Cross-platform sed (works on Mac too)
sed -i.bak "s/MANAGED-IDENTITY/$MANAGED_IDENTITY_NAME/g" script.sql && rm -f script.sql.bak
python3 run-sql-dbrole.py
echo "  ✓ Database roles configured"

# 11. Deploy stored procedures
echo ""
echo "Step 11: Deploying stored procedures..."
python3 run-sql-stored-procs.py
echo "  ✓ Stored procedures deployed"

# 12. Deploy app code
echo ""
echo "Step 12: Deploying application code..."
az webapp deploy \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_SERVICE_NAME" \
  --src-path ./app/app.zip
echo "  ✓ Application deployed"

echo ""
echo "============================================================"
echo " Deployment Complete!"
echo "============================================================"
echo " Navigate to: https://$APP_HOSTNAME/Index"
echo "============================================================"
