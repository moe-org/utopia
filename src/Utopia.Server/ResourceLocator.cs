// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core.IO;
using Zio;

namespace Utopia.Server;

public class ResourceLocator(string root, IFileSystem fileSystem) : Core.IO.ResourceLocator
{
    public override IFileSystem FileSystem => fileSystem;

    public override string RootDirectory => ((UPath)root).ToAbsolute().ToString();

    public override string? ServerDirectory => null;
}
