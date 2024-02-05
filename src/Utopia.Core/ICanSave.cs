// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Core;

/// <summary>
/// 代表一个可以“保存”的类，但是我们并不关心保存到哪里。
/// </summary>
public interface ICanSave
{

    void Save();
}

/// <summary>
/// 代表一个可以保存为指定类型的类。
/// </summary>
public interface ICanSave<out T>
{
    T SaveAs();
}
