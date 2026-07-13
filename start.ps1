# ============================================================
# Knowverse Portal — Startup Script (Pure ASP.NET Core)
# Single server: C# backend serves BOTH frontend + API
# ============================================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Knowverse Portal — Starting Up..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Starting ASP.NET Core server..." -ForegroundColor Yellow
Write-Host "  > Frontend : http://localhost:5200" -ForegroundColor Green
Write-Host "  > API      : http://localhost:5200/api/opportunities" -ForegroundColor Green
Write-Host "  > API      : http://localhost:5200/api/jobs" -ForegroundColor Green
Write-Host "  > Health   : http://localhost:5200/health" -ForegroundColor Green
Write-Host ""
Write-Host "Press Ctrl+C to stop." -ForegroundColor Gray
Write-Host ""

Set-Location "$PSScriptRoot\backend"
dotnet run
