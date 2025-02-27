if (-not $env:WEBHOOK_URL) { goto file }

Invoke-RestMethod -Uri $env:WEBHOOK_URL -Method Post -ContentType "application/x-www-form-urlencoded" -Body @{
    success = "true"
    commit_id = $env:APPVEYOR_REPO_COMMIT.Substring(0, 7)
    project_name = $env:APPVEYOR_PROJECT_NAME
    build_url = "$env:APPVEYOR_URL/project/$env:APPVEYOR_ACCOUNT_NAME/$env:APPVEYOR_PROJECT_SLUG/builds/$env:APPVEYOR_BUILD_ID"
    author = "Vespura"
    commit_url = "https://github.com/$env:APPVEYOR_REPO_NAME/commit/$env:APPVEYOR_REPO_COMMIT"
    commit_message = $env:APPVEYOR_REPO_COMMIT_MESSAGE
    build_name = "$env:APPVEYOR_PROJECT_NAME-$env:APPVEYOR_BUILD_ID-$env:APPVEYOR_BUILD_NUMBER"
}
Write-Output "Success webhook sent."

:file
Write-Output "Past :file"

if (-not $env:DISCORD_FILE_WEBHOOK) { goto end }
Invoke-RestMethod -Uri $env:DISCORD_FILE_WEBHOOK -Method Post -InFile "vMenu-$env:VERSION_NAME.zip" -ContentType "multipart/form-data"
Write-Output "File webhook sent."

:end
Write-Output "Past :end"