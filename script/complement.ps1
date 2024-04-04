Register-ArgumentCompleter -Native -CommandName nuke -ScriptBlock {
    param($commandName, $wordToComplete, $cursorPosition)
    dotnet nuke :complete "$wordToComplete" | ForEach-Object {
        [System.Management.Automation.CompletionResult]::new($_, $_, 'ParameterValue', $_)
    }
}
