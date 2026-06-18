param(
    [string]$BaseUrl = "http://localhost:5000"
)

$ErrorActionPreference = "Stop"

Write-Host "Using API base URL: $BaseUrl"

$now = [DateTime]::UtcNow
$entryTime = $now.AddHours(-3)

$weatherBody = @{
    startUtc = $entryTime.ToString("o")
    endUtc = $entryTime.AddHours(1.5).ToString("o")
    isRainy = $true
} | ConvertTo-Json

$weather = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/weather/interval" -ContentType "application/json" -Body $weatherBody
Write-Host "Weather interval created: $($weather.id)"

$entryBody = @{
    vehiclePlate = "DEMO-$(Get-Random -Minimum 1000 -Maximum 9999)"
    userId = "demo-user"
    isContractUser = $false
    preferCoveredSpace = $false
    entryTimeUtc = $entryTime.ToString("o")
} | ConvertTo-Json

$entry = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/sessions/entry" -ContentType "application/json" -Body $entryBody
$sessionId = $entry.sessionId
Write-Host "Session created: $sessionId"

$paymentBody = @{
    paymentChannel = "FLOOR_MACHINE"
    paidAtUtc = $now.ToString("o")
} | ConvertTo-Json

$payment = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/sessions/$sessionId/payment" -ContentType "application/json" -Body $paymentBody
Write-Host "Payment charged: $($payment.chargedAmount), discount: $($payment.discountAmount)"

$exit = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/sessions/$sessionId/exit"
Write-Host "Exit result: canExit=$($exit.canExit), message=$($exit.message)"

$report = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/reports/monthly?year=$($now.Year)&month=$($now.Month)"
Write-Host "Monthly revenue: $($report.totalRevenue), discountedPayments: $($report.discountedPayments)"

$inventory = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/inventory"
Write-Host "Inventory free spaces: $($inventory.freeSpaces) / $($inventory.totalSpaces)"
