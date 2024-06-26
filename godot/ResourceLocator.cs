// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utopia.Core.IO;

namespace Utopia.Godot;
public class ResourceLocator(string root) : Core.IO.ResourceLocator
{
    public override string RootDirectory => Path.GetFullPath(root);

    public override string? ServerDirectory => IResourceLocator.DefaultServerDirectoryName;
}

