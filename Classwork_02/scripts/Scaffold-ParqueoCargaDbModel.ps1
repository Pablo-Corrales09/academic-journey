Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Assert-Variable {
    param(
        [string]$Name,
        [string]$Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "The PowerShell variable `$$Name is not defined in this terminal session."
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$mvcProject = Join-Path $repoRoot 'ParqueoCarga\ParqueoCarga.csproj'
$dbModelProject = Join-Path $repoRoot 'ParqueoCarga.DbModel\ParqueoCarga.DbModel.csproj'

Assert-Variable -Name 'UserName' -Value $UserName
Assert-Variable -Name 'Password' -Value $Password
Assert-Variable -Name 'myDatabaseName' -Value $myDatabaseName
Assert-Variable -Name 'ServerName' -Value $ServerName

$port = 3306
$serverVersion = '8.4.0-mysql'
$sslMode = if ([string]::IsNullOrWhiteSpace($myCACertificate)) { 'Required' } else { 'VerifyCA' }

$connectionString = "Server=$ServerName;Port=$port;Database=$myDatabaseName;User ID=$UserName;Password=$Password;SslMode=$sslMode;"
if (-not [string]::IsNullOrWhiteSpace($myCACertificate)) {
    $connectionString += "SslCa=$myCACertificate;"
}

$sharedSecrets = @{
    'DatabaseSettings:Server' = $ServerName
    'DatabaseSettings:Port' = $port
    'DatabaseSettings:Database' = $myDatabaseName
    'DatabaseSettings:User' = $UserName
    'DatabaseSettings:Password' = $Password
    'DatabaseSettings:SslMode' = $sslMode
    'DatabaseSettings:ServerVersion' = $serverVersion
}

if (-not [string]::IsNullOrWhiteSpace($myCACertificate)) {
    $sharedSecrets['DatabaseSettings:CaCertificatePath'] = $myCACertificate
}

foreach ($project in @($mvcProject, $dbModelProject)) {
    foreach ($entry in $sharedSecrets.GetEnumerator()) {
        dotnet user-secrets set --project $project $entry.Key "$($entry.Value)" | Out-Null
    }
}

dotnet user-secrets set --project $mvcProject 'ConnectionStrings:MyConnectionString' $connectionString | Out-Null
dotnet user-secrets set --project $dbModelProject 'ConnectionStrings:MyConnectionString' $connectionString | Out-Null

Set-Location (Split-Path -Parent $mvcProject)

dotnet tool run dotnet-ef dbcontext scaffold $connectionString Pomelo.EntityFrameworkCore.MySql `
    --project "..\ParqueoCarga.DbModel\ParqueoCarga.DbModel.csproj" `
    --context ParqueoCargaContext `
    --context-dir Data `
    --output-dir Models `
    --table prq_automoviles `
    --table prq_parqueo `
    --table prq_ingreso_automoviles `
    --no-onconfiguring `
    --force

Write-Host 'Secrets were stored and the DbModel project was scaffolded successfully.'