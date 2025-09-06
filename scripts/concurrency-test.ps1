# PowerShell script to test concurrency
param(
    [string]$BaseUrl = "https://localhost:7000",
    [int]$ConcurrentRequests = 20,
    [string]$ProductId = ""
)

if ([string]::IsNullOrEmpty($ProductId)) {
    Write-Host "Creating test product first..."
    
    $createProductBody = @{
        name = "Concurrency Test Product"
        description = "Product for testing concurrent orders"
        price = 99.99
        stockQuantity = 10
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/products" -Method POST -Body $createProductBody -ContentType "application/json"
        $ProductId = $response.id
        Write-Host "Created product with ID: $ProductId"
    }
    catch {
        Write-Host "Failed to create product: $_"
        exit 1
    }
}

Write-Host "Starting $ConcurrentRequests concurrent order requests..."

$jobs = @()
$orderBody = @{
    orderItems = @(
        @{
            productId = $ProductId
            quantity = 1
        }
    )
} | ConvertTo-Json -Depth 3

for ($i = 1; $i -le $ConcurrentRequests; $i++) {
    $job = Start-Job -ScriptBlock {
        param($url, $body)
        try {
            $response = Invoke-RestMethod -Uri $url -Method POST -Body $body -ContentType "application/json"
            return @{ Success = $true; Response = $response }
        }
        catch {
            return @{ Success = $false; Error = $_.Exception.Message }
        }
    } -ArgumentList "$BaseUrl/api/orders", $orderBody
    
    $jobs += $job
}

Write-Host "Waiting for all requests to complete..."
$results = $jobs | Wait-Job | Receive-Job

$successCount = ($results | Where-Object { $_.Success -eq $true }).Count
$failureCount = ($results | Where-Object { $_.Success -eq $false }).Count

Write-Host ""
Write-Host "Results:"
Write-Host "Successful orders: $successCount"
Write-Host "Failed orders: $failureCount"
Write-Host ""

if ($failureCount -gt 0) {
    Write-Host "Sample failure reasons:"
    $results | Where-Object { $_.Success -eq $false } | Select-Object -First 3 | ForEach-Object {
        Write-Host "- $($_.Error)"
    }
}

# Check final product stock
try {
    $product = Invoke-RestMethod -Uri "$BaseUrl/api/products/$ProductId" -Method GET
    Write-Host ""
    Write-Host "Final product stock: $($product.stockQuantity)"
    Write-Host "Expected stock: $($10 - $successCount)"
}
catch {
    Write-Host "Failed to get final product state: $_"
}

# Cleanup jobs
$jobs | Remove-Job