dotnet clean
dotnet pack SCI_Lib -o out
dotnet nuget push "out\*.nupkg" -s c:\NuGet
del /F/Q out\*