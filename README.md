# SharedKernel
https://www.nuget.org/profiles/OpenHRCoreOrg

nuget api key : ```----nuget_api_key---``` 

```
@echo off
setlocal

rem Prompt user for API key
set /p API_KEY=Enter your NuGet API Key: 

rem Iterate through each .nupkg file in the current directory
for %%f in (*.nupkg) do (
    if exist %%f (
        echo NuGet package file found: %%f
        echo Pushing NuGet Package...
        dotnet nuget push %%f --api-key %API_KEY% --source https://api.nuget.org/v3/index.json
        if %errorlevel% neq 0 (
            echo Failed to push the NuGet package: %%f
        ) else (
            echo NuGet package pushed successfully: %%f
        )
    ) else (
        echo ERROR: NuGet package file not found: %%f
    )
)

endlocal
pause

```
.NET CLI
```
dotnet nuget push OpenHRCore.SharedKernel.Utilities.1.0.1-beta.nupkg --api-key nuget_api_key --source https://api.nuget.org/v3/index.json
```
