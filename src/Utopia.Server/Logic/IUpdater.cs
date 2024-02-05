// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Server.Logic;

/// <summary>
/// 更新器
/// </summary>
public interface IUpdater
{
    public void AssignTask(Action task);
}

public interface IUpdatable
{
    public void Update(IUpdater updater);
}

public class SimplyUpdater : IUpdater
{
    public void AssignTask(Action task) => task.Invoke();
}
