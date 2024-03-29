
function Check {
    param (
        $program
    )
    
    & "$program" "--version"

    if (-not $?){
        Write-Host "failed to check executable ``$program``"
        exit 1
    }
}   

Check("dotnet")
