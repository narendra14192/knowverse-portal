# ============================================================
# Knowverse Portal — Full Stack Startup Script
# Launches: C# API Backend (port 5200) + Frontend Static Server (port 8080)
# ============================================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Knowverse Portal — Starting Up..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# --- Step 1: Start C# API Backend ---
Write-Host "[1/2] Starting C# API Gateway on http://127.0.0.1:5200..." -ForegroundColor Yellow
$backendJob = Start-Job -ScriptBlock {
    Set-Location "c:\Users\naren\OneDrive\Desktop\insta link page\backend"
    dotnet run
}
Start-Sleep -Seconds 3

# --- Step 2: Start Frontend Static Server ---
Write-Host "[2/2] Starting Frontend Static Server on http://localhost:8080..." -ForegroundColor Yellow

# Check if Node.js is available for static serving
$nodeAvailable = Get-Command node -ErrorAction SilentlyContinue
if ($nodeAvailable) {
    $serverScript = "$PSScriptRoot\frontend\server.js"
    if (Test-Path $serverScript) {
        $frontendJob = Start-Job -ScriptBlock {
            node "c:\Users\naren\OneDrive\Desktop\insta link page\frontend\server.js"
        }
    } else {
        Write-Host "   (No server.js found — open frontend/index.html in browser or use VS Code Live Server)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "   Portal Ready!" -ForegroundColor Green
Write-Host "   Frontend : http://localhost:8080" -ForegroundColor Green
Write-Host "   Backend  : http://127.0.0.1:5200" -ForegroundColor Green
Write-Host "   Health   : http://127.0.0.1:5200/health" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Press Ctrl+C to stop all services." -ForegroundColor Gray

# Keep script running
try {
    Wait-Job $backendJob
} finally {
    Stop-Job $backendJob -ErrorAction SilentlyContinue
    Remove-Job $backendJob -ErrorAction SilentlyContinue
    if ($frontendJob) {
        Stop-Job $frontendJob -ErrorAction SilentlyContinue
        Remove-Job $frontendJob -ErrorAction SilentlyContinue
    }
    Write-Host "All services stopped." -ForegroundColor Red
}
