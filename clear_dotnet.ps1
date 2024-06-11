Push-Location
Set-Location -Path ".\nuget"
if (Test-Path -Path "BertJapaneseTokenizer.*") {
    Remove-Item "BertJapaneseTokenizer.*" -Force
}
Pop-Location

Push-Location
Set-Location -Path ".\dotnet\BertJapaneseTokenizer"
if (Test-Path -Path ".\bin") {
    Remove-Item "bin" -Force -Recurse
}
if (Test-Path -Path ".\obj") {
    Remove-Item "obj" -Force -Recurse
}
Pop-Location
