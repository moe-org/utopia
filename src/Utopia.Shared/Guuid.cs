using System;
using System.Collections.Generic;
using System.Text;

namespace Utopia.Shared;

public static class GuuidStandard
{
    /// <summary>
    ///     Use this regex pattern to check the guuid string
    /// </summary>
    public const string Pattern = @"^[a-zA-Z]{1}[a-zA-Z0-9]*(\.[a-zA-Z]{1}[a-zA-Z0-9]*)+$";

    /// <summary>
    ///     utopia游戏所使用的GUUID的root.Do not use in your mod!
    /// </summary>
    public const string UtopiaRoot = "Utopia";

    /// <summary>
    ///     We will use this to separate root and each namespaces when use the string of Guuid.
    /// </summary>
    public const string Separator = ".";
}
