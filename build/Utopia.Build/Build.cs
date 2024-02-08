#region

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;

#endregion

partial class Build : NukeBuild
{
    AbsolutePath SlnFilePath => RootDirectory / "src" / "Utopia.sln";

    AbsolutePath GodotPath => RootDirectory / "godot";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration _configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetClean(config =>
            config.SetProject(SlnFilePath));
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(config =>
            config.SetProjectFile(SlnFilePath));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(config => config
            .SetProjectFile(SlnFilePath)
            .SetNoRestore(true));
        });

    Target Test => _ =>
        _.DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(config =>
            config
            .SetProjectFile(SlnFilePath)
            .SetNoBuild(true));
        });

    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);
}
