// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;

namespace Utopia.Server.Map;

/// <summary>
/// 世界工厂
/// </summary>
public class WorldFactory : IWorldFactory
{
    public required ResourceLocator ResourceLocator { get; init; }

    public Guuid WorldType => new Guuid("Utopia", "DefaultWorld");

    private readonly Generator _generator;

    public WorldFactory(Generator generator)
    {
        ArgumentNullException.ThrowIfNull(generator, nameof(generator));
        _generator = generator;
    }

    public IWorld GenerateNewWorld() => new World(new Guuid("Utopia", "NewWorld"), 4, 4, _generator,
                                                  Path.Join(ResourceLocator.WorldsDirectory, "NewWorld").ToString());
}
