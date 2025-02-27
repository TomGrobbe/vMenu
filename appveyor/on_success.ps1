# Check if $env:WEBHOOK_URL is defined

if ($env:WEBHOOK_URL) {
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
} else {
    Write-Output "No webhook URL defined, skipping webhook."
}

if ($env:DISCORD_FILE_WEBHOOK) {
    Invoke-RestMethod -Uri $env:DISCORD_FILE_WEBHOOK -Method Post -InFile "vMenu-$env:VERSION_NAME.zip" -ContentType "multipart/form-data"
    Write-Output "File webhook sent."
} else {
    Write-Output "No file webhook URL defined, skipping file webhook."
}