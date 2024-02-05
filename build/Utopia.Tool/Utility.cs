#region

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NLog;

#endregion

namespace Utopia.Tool;

public class Utility
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    public static Project[] OpenSlnToProject(string sln)
    {
        var msWorkspace = MSBuildWorkspace.Create();
        var solution = msWorkspace.OpenSolutionAsync(sln!);

        solution.Wait();

        return solution.Result.Projects.ToArray();
    }

    public static Project OpenProject(string project)
    {
        var msWorkspace = MSBuildWorkspace.Create();
        var t = msWorkspace.OpenProjectAsync(project);
        t.Wait();
        return t.Result;
    }

    public static Compilation[] GetCompilation(params Project[] projects)
    {
        List<Task<Compilation?>> compilations = new();

        foreach (var project in projects) compilations.Add(project.GetCompilationAsync());
        Task.WhenAll(compilations.ToArray()).Wait();

        List<Compilation> result = new();
        for (var index = 0; index != projects.Length; index++)
        {
            if (compilations[index].Result == null)
            {
                s_logger.Error("failed to compile project {project}", projects[index]);
                continue;
            }

            result.Add(compilations[index].Result!);
        }

        return result.ToArray();
    }
}