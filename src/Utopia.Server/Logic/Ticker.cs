// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Diagnostics;

namespace Utopia.Server.Logic;

/// <summary>
/// 游戏时间管理的核心，负责高精度计时，非线程安全
/// </summary>
public interface ITicker
{
    const int TicksPerSecond = 20;

    const int MillisecondPerTick = 1000 / TicksPerSecond;

    /// <summary>
    /// Start to tick. Never stop
    /// </summary>
    void Start();

    /// <summary>
    /// 计时
    /// </summary>
    void Tick();

    /// <summary>
    /// 从开始计时已经tick多少次了
    /// </summary>
    ulong TickCount { get; }

    long MillisecondFromLastTick { get; }

    void WaitToNextTick();
}

internal class Ticker : ITicker
{
    private readonly Stopwatch _stopwatch = new();

    private readonly SpinWait _spin = new();

    public ulong TickCount { get; private set; }

    public long MillisecondFromLastTick => _stopwatch.ElapsedMilliseconds;

    public void Start() => _stopwatch.Start();

    public void Tick()
    {
        TickCount++;
        _stopwatch.Restart();
    }

    public void WaitToNextTick()
    {
        while (MillisecondFromLastTick < ITicker.MillisecondPerTick)
        {
            _spin.SpinOnce();
        }
        _spin.Reset();
    }

}
