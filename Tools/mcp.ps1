# Cliente MCP mÃ­nimo (Streamable HTTP) para el servidor MCP for Unity.
# Uso:
#   .\Tools\mcp.ps1 -List                               # lista herramientas
#   .\Tools\mcp.ps1 -Tool read_console -Json '{"action":"get"}'
param(
    [switch]$List,
    [string]$Tool,
    [string]$Json = "{}",
    [string]$Url = "http://127.0.0.1:8080/mcp"
)

$ErrorActionPreference = "Stop"
$headers = @{ Accept = "application/json, text/event-stream" }

function Invoke-Rpc([hashtable]$body, [string]$session) {
    $h = $headers.Clone()
    if ($session) { $h["mcp-session-id"] = $session }
    $json = $body | ConvertTo-Json -Depth 50 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $r = Invoke-WebRequest -Uri $Url -Method Post -ContentType "application/json; charset=utf-8" -Headers $h -Body $bytes -UseBasicParsing -TimeoutSec 180
    $text = [System.Text.Encoding]::UTF8.GetString($r.RawContentStream.ToArray())
    # Respuesta SSE: tomar las lÃ­neas "data:"
    if ($text -match "(?m)^data:") {
        $text = (($text -split "`n") | Where-Object { $_ -like "data:*" } | Select-Object -Last 1).Substring(5).Trim()
    }
    [pscustomobject]@{ Session = $r.Headers["mcp-session-id"]; Body = $text }
}

$init = Invoke-Rpc @{ jsonrpc = "2.0"; id = 1; method = "initialize"; params = @{
    protocolVersion = "2025-03-26"; capabilities = @{}; clientInfo = @{ name = "kiro-cli"; version = "1.0" } } } $null
$sid = $init.Session
try { Invoke-Rpc @{ jsonrpc = "2.0"; method = "notifications/initialized" } $sid | Out-Null } catch {}

if ($List) {
    $res = (Invoke-Rpc @{ jsonrpc = "2.0"; id = 2; method = "tools/list" } $sid).Body | ConvertFrom-Json
    if ($Tool) { $res.result.tools | ? name -eq $Tool | ConvertTo-Json -Depth 20 } else { $res.result.tools | ForEach-Object { # Cliente MCP mÃ­nimo (Streamable HTTP) para el servidor MCP for Unity.
# Uso:
#   .\Tools\mcp.ps1 -List                               # lista herramientas
#   .\Tools\mcp.ps1 -Tool read_console -Json '{"action":"get"}'
param(
    [switch]$List,
    [string]$Tool,
    [string]$Json = "{}",
    [string]$Url = "http://127.0.0.1:8080/mcp"
)

$ErrorActionPreference = "Stop"
$headers = @{ Accept = "application/json, text/event-stream" }

function Invoke-Rpc([hashtable]$body, [string]$session) {
    $h = $headers.Clone()
    if ($session) { $h["mcp-session-id"] = $session }
    $json = $body | ConvertTo-Json -Depth 50 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $r = Invoke-WebRequest -Uri $Url -Method Post -ContentType "application/json; charset=utf-8" -Headers $h -Body $bytes -UseBasicParsing -TimeoutSec 180
    $text = [System.Text.Encoding]::UTF8.GetString($r.RawContentStream.ToArray())
    # Respuesta SSE: tomar las lÃ­neas "data:"
    if ($text -match "(?m)^data:") {
        $text = (($text -split "`n") | Where-Object { $_ -like "data:*" } | Select-Object -Last 1).Substring(5).Trim()
    }
    [pscustomobject]@{ Session = $r.Headers["mcp-session-id"]; Body = $text }
}

$init = Invoke-Rpc @{ jsonrpc = "2.0"; id = 1; method = "initialize"; params = @{
    protocolVersion = "2025-03-26"; capabilities = @{}; clientInfo = @{ name = "kiro-cli"; version = "1.0" } } } $null
$sid = $init.Session
try { Invoke-Rpc @{ jsonrpc = "2.0"; method = "notifications/initialized" } $sid | Out-Null } catch {}

if ($List) {
    $res = (Invoke-Rpc @{ jsonrpc = "2.0"; id = 2; method = "tools/list" } $sid).Body | ConvertFrom-Json
    $res.result.tools | ForEach-Object { $_.name }
    return
}

$argObj = $Json | ConvertFrom-Json
$ht = @{}
$argObj.PSObject.Properties | ForEach-Object { $ht[$_.Name] = $_.Value }
$res = (Invoke-Rpc @{ jsonrpc = "2.0"; id = 3; method = "tools/call"; params = @{ name = $Tool; arguments = $ht } } $sid).Body | ConvertFrom-Json
if ($res.error) { "ERROR: " + ($res.error | ConvertTo-Json -Depth 10); return }
$res.result.content | ForEach-Object { $_.text }

.name } }
    return
}

$argObj = $Json | ConvertFrom-Json
$ht = @{}
$argObj.PSObject.Properties | ForEach-Object { $ht[$_.Name] = $_.Value }
$res = (Invoke-Rpc @{ jsonrpc = "2.0"; id = 3; method = "tools/call"; params = @{ name = $Tool; arguments = $ht } } $sid).Body | ConvertFrom-Json
if ($res.error) { "ERROR: " + ($res.error | ConvertTo-Json -Depth 10); return }
$res.result.content | ForEach-Object { $_.text }


