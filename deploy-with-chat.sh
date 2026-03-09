#!/bin/bash
set -e

# ============================================================
# deploy-with-chat.sh  — Deploy Expense App WITH GenAI (chat)
# Usage: ./deploy-with-chat.sh
# ============================================================

RESOURCE_GROUP="rg-expensemgmt"
LOCATION="uksouth"
MANAGED_IDENTITY_NAME="mid-AppModAssist-$(date +%d-%H-%M)"
ADMIN_LOGIN="expenseadmin"

echo ""
echo "============================================================"
echo " Expense App — Azure Deployment WITH GenAI"
echo "============================================================"
echo " Resource Group : $RESOURCE_GROUP"
echo " Location       : $LOCATION"
echo " Identity       : $MANAGED_IDENTITY_NAME"
echo " GenAI          : ENABLED (swedencentral)"
echo "============================================================"
echo ""

# ── 1. Get current user's Object ID (for SQL Azure AD admin) ──
echo "[1/14] Getting Azure AD Object ID..."
ADMIN_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)
echo "       Admin Object ID: $ADMIN_OBJECT_ID"

# ── 2. Ensure resource group exists ───────────────────────────
echo "[2/14] Ensuring resource group '$RESOURCE_GROUP' exists..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none

# ── 3. Deploy Bicep infrastructure (with GenAI) ───────────────
echo "[3/14] Deploying Bicep infrastructure (including GenAI)..."
DEPLOY_OUTPUT=$(az deployment group create \
  --resource-group "$RESOURCE_GROUP" \
  --template-file infra/main.bicep \
  --parameters \
      managedIdentityName="$MANAGED_IDENTITY_NAME" \
      adminObjectId="$ADMIN_OBJECT_ID" \
      adminLogin="$ADMIN_LOGIN" \
      deployGenAI=true \
  --query "properties.outputs" \
  --output json)

echo "       Deployment complete."

# ── 4. Extract deployment outputs ────────────────────────────
echo "[4/14] Extracting deployment outputs..."
APP_SERVICE_NAME=$(echo "$DEPLOY_OUTPUT" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['appServiceName']['value'])")
APP_URL=$(echo "$DEPLOY_OUTPUT"         | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['appServiceUrl']['value'])")
SQL_SERVER=$(echo "$DEPLOY_OUTPUT"      | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['sqlServerFqdn']['value'])")
SQL_DATABASE=$(echo "$DEPLOY_OUTPUT"    | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['databaseName']['value'])")
MI_CLIENT_ID=$(echo "$DEPLOY_OUTPUT"    | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['managedIdentityClientId']['value'])")
MI_ID=$(echo "$DEPLOY_OUTPUT"           | python3 -c "import sys,json; d=json.load(sys.stdin); print(d['managedIdentityId']['value'])")
OPENAI_ENDPOINT=$(echo "$DEPLOY_OUTPUT" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('openAIEndpoint',{}).get('value',''))")
OPENAI_MODEL=$(echo "$DEPLOY_OUTPUT"    | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('openAIModelName',{}).get('value','gpt-4o'))")
SEARCH_ENDPOINT=$(echo "$DEPLOY_OUTPUT" | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('searchEndpoint',{}).get('value',''))")

echo "       App Service    : $APP_SERVICE_NAME"
echo "       App URL        : $APP_URL"
echo "       SQL Server     : $SQL_SERVER"
echo "       SQL Database   : $SQL_DATABASE"
echo "       MI Client ID   : $MI_CLIENT_ID"
echo "       OpenAI Endpoint: $OPENAI_ENDPOINT"
echo "       OpenAI Model   : $OPENAI_MODEL"
echo "       Search Endpoint: $SEARCH_ENDPOINT"

# ── 5. Update App Service connection string ───────────────────
echo "[5/14] Configuring App Service connection string..."
CONNECTION_STRING="Server=$SQL_SERVER;Database=$SQL_DATABASE;Authentication=Active Directory Managed Identity;User Id=$MI_CLIENT_ID;Encrypt=True;TrustServerCertificate=False;"
az webapp config connection-string set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_SERVICE_NAME" \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$CONNECTION_STRING" \
  --output none
echo "       Connection string updated."

# ── 6. Configure OpenAI and Search app settings ───────────────
echo "[6/14] Setting AI app settings on App Service..."
az webapp config appsettings set \
  --resource-group "$RESOURCE_GROUP" \
  --name "$APP_SERVICE_NAME" \
  --settings \
      "OpenAI__Endpoint=$OPENAI_ENDPOINT" \
      "OpenAI__ModelName=$OPENAI_MODEL" \
      "Search__Endpoint=$SEARCH_ENDPOINT" \
      "AZURE_CLIENT_ID=$MI_CLIENT_ID" \
      "ManagedIdentityClientId=$MI_CLIENT_ID" \
  --output none
echo "       AI settings configured."

# ── 7. Add current machine IP to SQL firewall ─────────────────
echo "[7/14] Adding firewall rule for current IP..."
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

# ── 8. Wait for SQL to be ready ───────────────────────────────
echo "[8/14] Waiting 30 seconds for SQL to be fully ready..."
sleep 30

# ── 9. Install Python dependencies ───────────────────────────
echo "[9/14] Installing Python dependencies..."
pip3 install --quiet pyodbc azure-identity

# ── 10. Run schema script ─────────────────────────────────────
echo "[10/14] Running database schema script..."
sed -i.bak "s/example.database.windows.net/$SQL_SERVER/g" run-sql.py
sed -i.bak "s/ExpenseMgmt/$SQL_DATABASE/g" run-sql.py
python3 run-sql.py
echo "        Schema applied."

# ── 11. Run managed identity DB role script ───────────────────
echo "[11/14] Granting managed identity database role..."
sed -i.bak "s/MANAGED-IDENTITY-NAME/$MANAGED_IDENTITY_NAME/g" script.sql
sed -i.bak "s/example.database.windows.net/$SQL_SERVER/g" run-sql-dbrole.py
sed -i.bak "s/ExpenseMgmt/$SQL_DATABASE/g" run-sql-dbrole.py
python3 run-sql-dbrole.py
echo "        DB role granted."

# ── 12. Run stored procedures script ─────────────────────────
echo "[12/14] Deploying stored procedures..."
sed -i.bak "s/example.database.windows.net/$SQL_SERVER/g" run-sql-stored-procs.py
sed -i.bak "s/ExpenseMgmt/$SQL_DATABASE/g" run-sql-stored-procs.py
python3 run-sql-stored-procs.py
echo "        Stored procedures deployed."

# ── 13. Build and deploy the app ─────────────────────────────
echo "[13/14] Building and deploying the application..."
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

echo "        App deployed."

# ── 14. Wait then verify app is responding ────────────────────
echo "[14/14] Waiting 30 seconds for app to start..."
sleep 30
echo "        Done."

echo ""
echo "============================================================"
echo " Deployment complete (with GenAI)!"
echo " App available at    : $APP_URL/Index"
echo " Swagger docs at     : $APP_URL/swagger"
echo " Chat assistant at   : $APP_URL/chatui/index.html"
echo " OpenAI endpoint     : $OPENAI_ENDPOINT"
echo "============================================================"
echo ""
