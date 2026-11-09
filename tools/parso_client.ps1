# Helpers to talk to Parso from Windows PowerShell, no Python needed.
# Load them in the window you test from:  . .\tools\parso_client.ps1
# If PowerShell refuses to run the file:  Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
#
#   Send-Parso '{"2":"STATUS"}'
#   Send-ParsoPayment '{"3":{"amountInPennies":30,"clientTransactionID":"T1","testEth":false}}'
#
# Both frame the request as a 4-byte big-endian length plus UTF-8 text, like Parso expects.
# Send-Parso prints the first frame that comes back. Send-ParsoPayment prints every frame with
# its time (the CARDINSERTED broadcast included) until the reply arrives, so give it a long timeout.

function Read-ParsoBytes($stream, [int]$count) {
    $buf = New-Object byte[] $count
    $got = 0
    while ($got -lt $count) { $r = $stream.Read($buf, $got, $count - $got); if ($r -le 0) { throw 'connection closed' }; $got += $r }
    return ,$buf
}

function Send-ParsoFrame($stream, [string]$Json) {
    $payload = [System.Text.Encoding]::UTF8.GetBytes($Json)
    $len = [System.BitConverter]::GetBytes([System.Net.IPAddress]::HostToNetworkOrder([int]$payload.Length))
    $stream.Write($len, 0, 4)
    $stream.Write($payload, 0, $payload.Length)
}

function Read-ParsoFrame($stream) {
    $n = [System.Net.IPAddress]::NetworkToHostOrder([System.BitConverter]::ToInt32((Read-ParsoBytes $stream 4), 0))
    return [System.Text.Encoding]::UTF8.GetString((Read-ParsoBytes $stream $n))
}

function Send-Parso([string]$Json, [int]$TimeoutSec = 20, [string]$HostName = '127.0.0.1', [int]$Port = 1994) {
    $client = New-Object System.Net.Sockets.TcpClient($HostName, $Port)
    try {
        $stream = $client.GetStream()
        $stream.ReadTimeout = $TimeoutSec * 1000
        Send-ParsoFrame $stream $Json
        Read-ParsoFrame $stream
    } finally { $client.Close() }
}

function Send-ParsoPayment([string]$Json, [int]$TimeoutSec = 90, [string]$HostName = '127.0.0.1', [int]$Port = 1994) {
    $client = New-Object System.Net.Sockets.TcpClient($HostName, $Port)
    try {
        $stream = $client.GetStream()
        $stream.ReadTimeout = $TimeoutSec * 1000
        Send-ParsoFrame $stream $Json
        while ($true) {
            $text = Read-ParsoFrame $stream
            Write-Output ("[{0:HH:mm:ss.fff}] {1}" -f (Get-Date), $text)
            if ($text.StartsWith('{') -and $text.Contains('"Success"')) { break }
        }
    } finally { $client.Close() }
}
