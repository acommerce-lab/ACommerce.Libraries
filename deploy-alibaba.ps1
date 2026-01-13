# ============================================
# Alibaba Local Deploy Script (Windows)
# ============================================

$ErrorActionPreference = "Stop"

# ============== CONFIG ======================
$ImageName  = "ashare-api"
$ImageTag   = "latest"
$TarFile    = "ashare-api.tar"

$EcsUser    = "ecs-user"
$EcsHost    = "8.213.82.216"
$SshKey     = "C:\Users\i\.ssh\ashare-key.pem"

$RemoteDir  = "/home/ecs-user"
# ============================================

function Info($msg) {
    Write-Host "[INFO] $msg" -ForegroundColor Green
}

function Error($msg) {
    Write-Host "[ERROR] $msg" -ForegroundColor Red
}

# -------- STEP 1: BUILD ---------------------
Info "Step 1: Building Docker image (no cache)..."
docker build --no-cache -t "${ImageName}:${ImageTag}" -f Apps/Ashare.Api/Dockerfile .

if ($LASTEXITCODE -ne 0) {
    Error "Docker build failed"
    exit 1
}
Info "Docker image built successfully"

# -------- STEP 2: SAVE ----------------------
Info "Step 2: Saving Docker image to TAR..."
docker save -o $TarFile "${ImageName}:${ImageTag}"
Info "Image saved as $TarFile"

# -------- STEP 3: COPY TAR ------------------
Info "Step 3: Copying TAR file to ECS..."
scp -i $SshKey $TarFile "${EcsUser}@${EcsHost}:${RemoteDir}/"
Info "TAR file copied successfully"

# -------- STEP 4: DEPLOY --------------------
Info "Step 4: Running deploy on ECS..."

# Using single-line commands to avoid \r issues
ssh -i $SshKey "${EcsUser}@${EcsHost}" "docker stop ashare-api 2>/dev/null || true"
ssh -i $SshKey "${EcsUser}@${EcsHost}" "docker rm ashare-api 2>/dev/null || true"
ssh -i $SshKey "${EcsUser}@${EcsHost}" "docker image rm ashare-api:latest 2>/dev/null || true"

Info "Loading new image..."
ssh -i $SshKey "${EcsUser}@${EcsHost}" "docker load -i /home/ecs-user/ashare-api.tar"

Info "Starting container..."
ssh -i $SshKey "${EcsUser}@${EcsHost}" "docker run -d --name ashare-api --env-file /home/ecs-user/ashare.env -p 8080:8080 --restart unless-stopped ashare-api:latest"

# -------- STEP 5: VERIFY --------------------
Info "Step 5: Waiting for startup..."
Start-Sleep -Seconds 5

Info "Checking container status..."
ssh -i $SshKey "${EcsUser}@${EcsHost}" "docker ps | grep ashare-api"

Info "Checking logs..."
ssh -i $SshKey "${EcsUser}@${EcsHost}" "docker logs --tail 20 ashare-api"

Info "Testing health endpoint..."
ssh -i $SshKey "${EcsUser}@${EcsHost}" "curl -s http://localhost:8080/health"

Info "========================================"
Info "Deployment completed successfully!"
Info "========================================"
