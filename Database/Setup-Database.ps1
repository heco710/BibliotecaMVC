#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$Server = '.\SQLEXPRESS',
    [ValidatePattern('^[A-Za-z][A-Za-z0-9_]{0,63}$')][string]$DatabaseName = 'BibliotecaDB',
    [switch]$UseSqlAuthentication,
    [Security.SecureString]$AppPassword,
    [switch]$SaveConnectionString
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data
$adminBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$adminBuilder['Data Source'] = $Server
$adminBuilder['Initial Catalog'] = 'master'
$adminBuilder['Integrated Security'] = $true
$adminBuilder['Encrypt'] = $true
$adminBuilder['TrustServerCertificate'] = $true
$adminBuilder['Connect Timeout'] = 5
$connection = New-Object System.Data.SqlClient.SqlConnection $adminBuilder.ConnectionString
$plainPassword = $null
try {
    $connection.Open()
    if ($UseSqlAuthentication) {
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT CAST(SERVERPROPERTY('IsIntegratedSecurityOnly') AS int)"
        if ($command.ExecuteScalar() -eq 1) {
            throw 'SQL Server solo permite autenticación Windows. Habilite modo mixto y reinicie con autorización, o ejecute sin -UseSqlAuthentication.'
        }
    }
    $schema = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'BibliotecaDB.sql')).Replace('$(DatabaseName)', $DatabaseName)
    foreach ($batch in [regex]::Split($schema, '(?im)^GO\s*$')) {
        if ([string]::IsNullOrWhiteSpace($batch)) { continue }
        $command = $connection.CreateCommand()
        $command.CommandText = $batch
        [void]$command.ExecuteNonQuery()
        $command.Dispose()
    }
    $appBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $adminBuilder.ConnectionString
    $appBuilder['Initial Catalog'] = $DatabaseName
    if ($UseSqlAuthentication) {
        if ($AppPassword) {
            $plainPassword = [Net.NetworkCredential]::new('', $AppPassword).Password
        } else {
            $command = $connection.CreateCommand()
            $command.CommandText = "SELECT SUSER_ID(N'biblioteca_user')"
            if ($command.ExecuteScalar() -isnot [DBNull]) {
                throw 'El login ya existe. Proporcione su contraseña con -AppPassword (Read-Host -AsSecureString). No se restablecen contraseñas.'
            }
            $bytes = New-Object byte[] 32
            $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
            $rng.GetBytes($bytes)
            $rng.Dispose()
            $plainPassword = 'B!9a' + [Convert]::ToBase64String($bytes)
            if (!$SaveConnectionString) { throw 'Para un login nuevo use -SaveConnectionString o proporcione -AppPassword; no se imprimen contraseñas.' }
        }
        $command = $connection.CreateCommand()
        $command.CommandText = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'CreateUser.sql'))
        [void]$command.Parameters.Add('@Password', [Data.SqlDbType]::NVarChar, 128)
        $command.Parameters['@Password'].Value = $plainPassword
        [void]$command.ExecuteNonQuery()
        $command.Dispose()
        $appBuilder['Integrated Security'] = $false
        $appBuilder['User ID'] = 'biblioteca_user'
        $appBuilder['Password'] = $plainPassword
    }
    $probe = New-Object System.Data.SqlClient.SqlConnection $appBuilder.ConnectionString
    try {
        $probe.Open()
        $command = $probe.CreateCommand()
        $command.CommandText = 'SELECT COUNT(*) FROM dbo.Categorias'
        $count = $command.ExecuteScalar()
        $command.Dispose()
    } finally { $probe.Dispose() }
    if ($SaveConnectionString) {
        if ($DatabaseName -ne 'BibliotecaDB') { throw 'Solo BibliotecaDB puede guardarse en la configuración de desarrollo.' }
        $secretsDirectory = Join-Path $env:APPDATA 'Microsoft\UserSecrets\BibliotecaMVC-semana-8'
        [void][IO.Directory]::CreateDirectory($secretsDirectory)
        $secretsPath = Join-Path $secretsDirectory 'secrets.json'
        $secrets = @{}
        if (Test-Path -LiteralPath $secretsPath) {
            $existing = Get-Content -LiteralPath $secretsPath -Raw | ConvertFrom-Json
            foreach ($property in $existing.PSObject.Properties) { $secrets[$property.Name] = $property.Value }
        }
        $secrets['ConnectionStrings:BibliotecaDB'] = $appBuilder.ConnectionString
        [IO.File]::WriteAllText($secretsPath, ($secrets | ConvertTo-Json), [Text.UTF8Encoding]::new($false))
    }
    Write-Output "Base preparada: $DatabaseName; categorias: $count; configuración guardada: $SaveConnectionString."
} catch {
    # Evita que PowerShell imprima detalles del proveedor que puedan incluir credenciales.
    if ($_.Exception -is [System.Management.Automation.RuntimeException] -and !$_.Exception.InnerException) { throw }
    throw 'No se pudo preparar la base de datos. Revise servidor, autenticación, permisos y compatibilidad del esquema. No se imprimen credenciales.'
} finally {
    $connection.Dispose()
    $plainPassword = $null
}
