// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utopia.Core.Plugin;

namespace Utopia.Core.IO;

public class PluginFileSystem(
    IFileSystem fileSystem,
    IPluginInformation information,
    string rootDirectory) : IPluginFileSystem
{
    public IFileSystem GameRootFileSystem { get; init; } = fileSystem;

    public string RootDirectory =>
        Path.GetFullPath(rootDirectory);

    public string GlobalConfigurationDirectory =>
        fileSystem.GetConfigurationDirectoryOfPlugin(information);
}
