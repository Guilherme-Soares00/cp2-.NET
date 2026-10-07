param([string]$BaseUri = 'http://localhost:5080')

$ErrorActionPreference = 'Stop'
$BaseUri = $BaseUri.TrimEnd('/')
$script:checks = 0

function Request([string]$Method, [string]$Path, [int]$Expected, $Body = $null) {
    $parameters = @{
        Uri = "$BaseUri$Path"
        Method = $Method
        SkipHttpErrorCheck = $true
    }
    if ($null -ne $Body) {
        $parameters.ContentType = 'application/json; charset=utf-8'
        $parameters.Body = ($Body | ConvertTo-Json -Compress)
    }
    $response = Invoke-WebRequest @parameters
    if ([int]$response.StatusCode -ne $Expected) {
        throw "$Method $Path retornou $($response.StatusCode), esperado $Expected. $($response.Content)"
    }
    $script:checks++
    return $response
}

$genreId = $null
try {
    Request 'GET' '/health' 200 | Out-Null
    Request 'GET' '/openapi/v1.json' 200 | Out-Null
    $name = "Verificacao-$([Guid]::NewGuid().ToString('N'))"
    $created = Request 'POST' '/api/genres' 201 @{ name = $name; description = $null }
    $genre = $created.Content | ConvertFrom-Json
    $genreId = $genre.id
    if (-not $genreId -or -not $created.Headers.Location) { throw 'Criação sem ID ou Location.' }
    $read = (Request 'GET' "/api/genres/$genreId" 200).Content | ConvertFrom-Json
    if ($read.name -ne $name -or $null -ne $read.description) { throw 'Dados lidos diferem dos gravados.' }
    $list = (Request 'GET' '/api/genres' 200).Content | ConvertFrom-Json
    if (-not ($list | Where-Object { $_.id -eq $genreId })) { throw 'Registro ausente da listagem.' }
    Request 'POST' '/api/genres' 409 @{ name = $name } | Out-Null
    $updated = (Request 'PUT' "/api/genres/$genreId" 200 @{ name = $name; description = 'Descrição atualizada' }).Content | ConvertFrom-Json
    if ($updated.description -ne 'Descrição atualizada') { throw 'Atualização não persistiu.' }
    $readUpdated = (Request 'GET' "/api/genres/$genreId" 200).Content | ConvertFrom-Json
    if ($readUpdated.description -ne 'Descrição atualizada') { throw 'Nova leitura não confirmou atualização.' }
    Request 'POST' '/api/genres' 400 @{ name = '   ' } | Out-Null
    Request 'POST' '/api/genres' 400 @{ name = ('x' * 101) } | Out-Null
    Request 'POST' '/api/genres' 400 @{ name = 'Teste'; description = ('x' * 501) } | Out-Null
    $missing = [Guid]::NewGuid()
    Request 'GET' "/api/genres/$missing" 404 | Out-Null
    Request 'PUT' "/api/genres/$missing" 404 @{ name = 'Ausente' } | Out-Null
    Request 'DELETE' "/api/genres/$missing" 404 | Out-Null
    Request 'DELETE' "/api/genres/$genreId" 204 | Out-Null
    Request 'GET' "/api/genres/$genreId" 404 | Out-Null
    Request 'DELETE' "/api/genres/$genreId" 404 | Out-Null
    $genreId = $null
    Write-Output "$script:checks verificações HTTP passaram."
}
finally {
    if ($genreId) {
        Invoke-WebRequest -Method Delete -Uri "$BaseUri/api/genres/$genreId" -SkipHttpErrorCheck | Out-Null
    }
}
