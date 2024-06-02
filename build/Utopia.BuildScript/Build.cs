using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using Utopia.BuildScript;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Release);

    AbsolutePath GodotProjectPath = RootDirectory / "godot";

    AbsolutePath SlnPath = RootDirectory / "src" / "Utopia.sln";

    AbsolutePath ReleasePath = RootDirectory / "release";

    AbsolutePath CorePath = RootDirectory / "src" / "Utopia.Core" / "Utopia.Core.csproj";

    AbsolutePath ClientPath = RootDirectory / "godot" / "Utopia.Godot.csproj";

    AbsolutePath ServerPath = RootDirectory / "src" / "Utopia.Server" / "Utopia.Server.csproj";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("The path to the godot engine")]
    readonly string? Godot = Utility.FindProgram("godot", "mono", "stable");

    Target Clean => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetClean();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore();
            DotNetTasks.DotNetToolRestore();
        });

    Target UpdateVersion => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNet("dotnet-gitversion /updateassemblyinfo");
        });

    Target CompileDotnet => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(config => config.SetProjectFile(SlnPath).SetConfiguration(Configuration.ToString()).SetNoRestore(true));
        });

    Target ReleaseGodot => _ => _
        .DependsOn(Restore)
        .Requires(() => Godot != null)
        .Executes(() =>
        {
            Log.Information("Use Godot Engine from:{}", Godot);

            ReleasePath.CreateOrCleanDirectory();
            (ReleasePath / "windows").CreateOrCleanDirectory();
            (ReleasePath / "linux").CreateOrCleanDirectory();

            ProcessTasks.StartProcess(Godot, $"--headless --path \"{GodotProjectPath}\" --export-release \"Windows Desktop\" \"{ReleasePath / "windows/utopia.exe"}\" --quit").WaitForExit();
            ProcessTasks.StartProcess(Godot, $"--headless --path \"{GodotProjectPath}\" --export-release \"Linux/X11\" \"{ReleasePath / "linux/utopia.x86_64"}\" --quit").WaitForExit();
        });

    Target Test => _ => _
        .DependsOn(CompileDotnet)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(config => config.SetNoBuild(true).SetNoRestore(true));
        });

    Target Release => _ => _
        .DependsOn(UpdateVersion, CompileDotnet);
}
