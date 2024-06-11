Push-Location

Set-Location -Path ".\dotnet\BertJapaneseTokenizer"
dotnet build --configuration Release
Copy-Item "bin\Release\BertJapaneseTokenizer.*.nupkg" "..\..\nuget"
Pop-Location

