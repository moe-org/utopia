// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Utopia.Core;
public static class TimeUtilities
{
    /// <summary>
    /// 在<see cref="CancellationToken"/> 取消的时候打印提示信息。
    /// </summary>
    /// <param name="logger">要输出信息的logger</param>
    /// <param name="name">提示信息</param>
    /// <param name="source">取消token</param>
    public static void SetAnNoticeWhenCancel(ILogger logger, string name, CancellationToken source)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        source.Register(() =>
        {
            stopwatch.Stop();
            logger.LogInformation(
                "{name} completed,using {seconds} s", name, stopwatch.Elapsed.TotalSeconds);
        });
    }
    public static void SetAnNoticeWhenComplete(ILogger logger, string name, Task source)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        Task.Run(async () =>
        {
            while (!source.IsCompleted)
            {
                await Task.Delay(50);
            }
            stopwatch.Stop();
            logger.LogInformation(
                "{name} completed,using {seconds} s", name, stopwatch.Elapsed.TotalSeconds);
        });
    }

}
