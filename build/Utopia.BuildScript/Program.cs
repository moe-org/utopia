// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.BuildScript;
public class Program
{
    public static int Main(string[] args)
    {

        int index = 0;
        while(index < args.Length)
        {
            var arg = args[index++];
            var narg = index < args.Length ? args[index] : null!;

            if(arg == "update_version")
            {

            }

        }

        return 0;
    }
}
