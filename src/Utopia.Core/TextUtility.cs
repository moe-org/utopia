// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core;
public static class TextUtility
{

    public static bool IsFileSUffixTypo(string file, string suffix) => FileNameSuffixTypoPossibility(file, suffix) > 70;

    public static int FileNameSuffixTypoPossibility(string file, string suffix)
    {
        file = file.Replace(".", " ");
        suffix = suffix.Replace(".", " ");

        return FuzzySharp.Fuzz.PartialRatio(file, suffix);
    }
}
