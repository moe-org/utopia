// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Formats.Tar;
using System.IO.Compression;
using SharpCompress.Archives.Tar;
using static SimpleExec.Command;

namespace Utopia.Tool;

/// <summary>
/// A plugin project
/// </summary>
public class PluginProject
{
    public string Root { get; }

    public PluginInformation Information { get; set; }

    public string ReleaseDirectory { get; set; }

    public string ReleaseFile { get; set; }

    public PluginProject(string projectRoot,PluginInformation information)
    {
        Root = Path.GetFullPath(projectRoot + "/");
        Information = information;
        ReleaseDirectory = Path.Join(Root, "release/");
        ReleaseFile = Path.Join(ReleaseDirectory, $"{information.Id.Guuid.ToCsIdentifier()}-{information.Version}.tar.zstd");
    }

    public void ReleaseTo(string? typename = null)
    {
        var csproj = Directory.GetFiles(Root).TakeWhile(file => Path.GetExtension(file) is ".csproj").First();

        Run("dotnet",[
            "public",
            csproj,
            "--configuration","Release",
             "--framework","net8.0",
            "--output",Path.GetFullPath(ReleaseDirectory),
            "--no-self-contained"]);

        var project = Utility.OpenProject(csproj);

        typename ??= (project.DefaultNamespace != null ? project.DefaultNamespace + '.' : string.Empty)
                     + "Plugin";

        Information.ToManifest([$"{project.Name}.dll"], typename);
    }

    public void Pack()
    {
        MemoryStream memory = new();
        TarFile.CreateFromDirectory(ReleaseDirectory, memory, false);

        if (File.Exists(ReleaseFile))
        {
            File.Delete(ReleaseFile);
        }
        using var fs = File.OpenWrite(ReleaseFile);
        using GZipStream stream = new(fs, CompressionLevel.Fastest, false);

        stream.Write(memory.ToArray());
    }
}
