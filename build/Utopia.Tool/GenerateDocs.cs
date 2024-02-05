#region

using Docfx;
using Docfx.Dotnet;
using Markdig;

#endregion

namespace Utopia.Tool;

/// <summary>
///     生成文档
/// </summary>
public static class GenerateDocs
{
    public static void Generate(string configFile)
    {
        var options = new BuildOptions
        {
            // Enable custom markdown extensions here
            ConfigureMarkdig = pipeline => pipeline.UseCitations()
        };

        DotnetApiCatalog.GenerateManagedReferenceYamlFiles(configFile).Wait();
        Docset.Build(configFile, options).Wait();
    }
}