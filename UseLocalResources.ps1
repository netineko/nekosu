$CSPROJ="osu.Game/osu.Game.csproj"
$SLN="osu.sln"

dotnet remove $CSPROJ package ppy.osu.Game.Resources;
dotnet sln $SLN add ../nekosu-resources/osu.Game.Resources/osu.Game.Resources.csproj
dotnet add $CSPROJ reference ../nekosu-resources/osu.Game.Resources/osu.Game.Resources.csproj

$SLNF=Get-Content "osu.Desktop.slnf" | ConvertFrom-Json
$TMP=New-TemporaryFile
$SLNF.solution.projects += ("../nekosu-resources/osu.Game.Resources/osu.Game.Resources.csproj")
ConvertTo-Json $SLNF | Out-File $TMP -Encoding UTF8
Move-Item -Path $TMP -Destination "osu.Desktop.slnf" -Force
