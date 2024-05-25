// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.BuildScript;
public static class Utility
{
    public static string? FindProgram(params string[] featureNames)
    {
        var env = Environment.GetEnvironmentVariable("PATH");
        string[] path;

        if (OperatingSystem.IsWindows())
        {
            path = env.Split(';');
        }
        else
        {
            path = env.Split(":");
        }

        foreach (var p in path)
        {
            foreach (var f in Directory.GetFiles(p))
            {
                bool match = true;
                foreach (var feature in featureNames)
                {
                    if (!f.Contains(feature))
                    {
                        match = false;
                    }
                }

                if (match)
                    return Path.GetFullPath(Path.Join(p, f));
            }
        }

        return null;
    }
}
