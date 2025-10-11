# Script to add copyright headers to source files

$csHeader = @"
// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

"@

$tsHeader = @"
// Copyright (c) Core. All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

"@

function Add-CopyrightHeader {
    param (
        [string]$FilePath,
        [string]$Header
    )
    
    $content = Get-Content -Path $FilePath -Raw -ErrorAction SilentlyContinue
    
    if ($null -eq $content) {
        Write-Host "Skipping empty or unreadable file: $FilePath"
        return
    }
    
    # Check if header already exists
    if ($content -match "Copyright \(c\)") {
        Write-Host "Header already exists in: $FilePath"
        return
    }
    
    # Add header
    $newContent = $Header + $content
    Set-Content -Path $FilePath -Value $newContent -NoNewline
    Write-Host "Added header to: $FilePath"
}

# Get all C# files
$csFiles = git ls-files "src/**/*.cs"
foreach ($file in $csFiles) {
    Add-CopyrightHeader -FilePath $file -Header $csHeader
}

# Get all TypeScript/TSX files
$tsFiles = git ls-files "src/**/*.ts" "src/**/*.tsx"
foreach ($file in $tsFiles) {
    Add-CopyrightHeader -FilePath $file -Header $tsHeader
}

Write-Host "`nCopyright headers added successfully!"

