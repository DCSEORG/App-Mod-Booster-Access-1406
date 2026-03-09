#!/bin/bash
set -e

# ============================================================
# deploy.sh  — Deploy Expense Management App (no GenAI)
# Usage: ./deploy.sh
# ============================================================

RESOURCE_GROUP="rg-expensemgmt"
LOCATION="uksouth"
MANAGED_IDENTITY_NAME="mid-AppModAssist-$(date +%d-%H-%M)"
ADMIN_LOGIN="expenseadmin"

echo ""
echo "============================================================"
echo " Expense App — Azure Deployment"
echo "============================================================"
echo " Resource Group : $RESOURCE_GROUP"
echo " Location       : $LOCATION"
echo " Identity       : $MANAGED_IDENTITY_NAME"
echo "============================================================"
echo ""

# ── 1. Get current user's Object ID (for SQL Azure AD admin) ──
echo "[1/12] Getting Azure AD Object ID..."
ADMIN_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)
echo "       Admin Object ID: $ADMIN_OBJECT_ID"

# ── 2. Ensure resource group exists ───────────────────────────
echo "[2/12] Ensuring resource group '$RESOURCE_GROUP' exists..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none

# ── 3. Deploy Bicep infrastructure (no GenAI) ─────────────────
echo "[3/12] Deploying Bicep infrastructure..."
DEPLOY_OUTPUT=$(az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/main.bicep \
  --parameters \
      managedIdentityName="$MANAGED_IDENTITY_NAME" \
      adminObjectId="$ADMIN_OBJECT_ID" \
      adminLogin="$ADMIN_LOGIN" \
      deployGenAI=false \
  --query "properties.outputs" \
  --output json)

echo "       Deployment complete."

# ── 4. Extract deployment outputs ────────────────────────────
echo "[4/12] Extracting deployment outputs..."
APP_SERVICE_NAME=$(echo "$DEPLOY_OUTPUT" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['appServiceName']['value'])")
APP_URL=$(echo "$DEPLOY_OUTPUT"         | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['appServiceUrl']['value'])")
SQL_SERVER=$(echo "$DEPLOY_OUTPUT"      | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['sqlServerFqdn']['value'])")
SQL_DATABASE=$(echo "$DEPLOY_OUTPUT"    | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['databaseName']['value'])")
MI_CLIENT_ID=$(echo "$DEPLOY_OUTPUT"    | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['managedIdentityClientId']['value'])")
MI_ID=$(echo "$DEPLOY_OUTPUT"           | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['managedIdentityId']['value'])")

echo "       App Service : $APP_SERVICE_NAME"
echo "       App URL     : $APP_URL"
echo "       SQL Server  : $SQL_SERVER"
echo "       SQL Database: $SQL_DATABASE"
echo "       MI Client ID: $MI_CLIENT_ID"

# ── 5. Update App Service connection string with MI Client ID ─
echo "[5/12] Configuring App Service connection string..."
CONNECTION_STRING="Server=$SQL_SERVER;Database=$SQL_DATABASE;Authentication=Active Directory Managed Identity;User Id=$MI_CLIENT_ID;Encrypt=True;TrustServerCertificate=False;"
az webapp config connection-string set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_SERVICE_NAME" \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$CONNECTION_STRING" \
  --output none
echo "       Connection string updated."

# ── 6. Add current machine IP to SQL firewall ─────────────────
echo "[6/12] Adding firewall rule for current IP..."
MY_IP=$(curl -s https://api.ipify.org)
echo "       Your IP: $MY_IP"
az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$(echo "$SQL_SERVER" | cut -d'.' -f1)" \
  --name "DeployMachine-$(date +%Y%m%d%H%M%S)" \
  --start-ip-address "$MY_IP" \
  --end-ip-address "$MY_IP" \
  --output none
echo "       Firewall rule added."

# ── 7. Wait for SQL to be ready ───────────────────────────────
echo "[7/12] Waiting 30 seconds for SQL to be fully ready..."
sleep 30

# ── 8. Install Python dependencies ───────────────────────────
echo "[8/12] Installing Python dependencies..."
pip3 install --quiet pyodbc azure-identity

# ── 9. Run schema script ──────────────────────────────────────
echo "[9/12] Running database schema script..."
sed -i.bak "s/example.database.windows.net/$SQL_SERVER/g" run-sql.py
sed -i.bak "s/ExpenseMgmt/$SQL_DATABASE/g" run-sql.py
python3 run-sql.py
echo "       Schema applied."

# ── 10. Run managed identity DB role script ───────────────────
echo "[10/12] Granting managed identity database role..."
sed -i.bak "s/MANAGED-IDENTITY-NAME/$MANAGED_IDENTITY_NAME/g" script.sql
sed -i.bak "s/example.database.windows.net/$SQL_SERVER/g" run-sql-dbrole.py
sed -i.bak "s/ExpenseMgmt/$SQL_DATABASE/g" run-sql-dbrole.py
python3 run-sql-dbrole.py
echo "       DB role granted."

# ── 11. Run stored procedures script ─────────────────────────
echo "[11/12] Deploying stored procedures..."
sed -i.bak "s/example.database.windows.net/$SQL_SERVER/g" run-sql-stored-procs.py
sed -i.bak "s/ExpenseMgmt/$SQL_DATABASE/g" run-sql-stored-procs.py
python3 run-sql-stored-procs.py
echo "       Stored procedures deployed."

# ── 12. Build and deploy the app ─────────────────────────────
echo "[12/12] Building and deploying the application..."
cd app
dotnet publish -c Release -o ./publish --nologo -v quiet
cd publish
zip -r ../../app.zip . -x "*.pdb"
cd ../..

az webapp deploy \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_SERVICE_NAME" \
  --src-path ./app.zip \
  --type zip \
  --output none

echo "       App deployed."

echo ""
echo "============================================================"
echo " Deployment complete!"
echo " App available at: $APP_URL/Index"
echo " Swagger docs at : $APP_URL/swagger"
echo "============================================================"
echo ""
