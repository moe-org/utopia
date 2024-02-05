// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core.Map;
using Utopia.Core.Translation;
using Utopia.Core.Utilities;
using Utopia.Server.Entity;

namespace Utopia.Server.Plugin.Entity;

/// <summary>
/// 草地实体，曾经一段时间被用于测试
/// </summary>
public class GrassEntity : IEntity
{
    public string Name { get; init; }

    public string Description => "no";

    public bool Accessible => false;

    public bool CanCollide => false;

    public Guuid Id => ResourcePack.Entity.GrassEntity.ID;

    public WorldPosition WorldPosition { get; set; }

    public GrassEntity()
    {
        //        this.Name = new ICUTranslatedString(ResourcePack.Entity.GrassEntity.TranslateKey, msg, id, new object());
        WorldPosition = new WorldPosition(1, 1, 1, 1);
        Name = "translated";
    }

    public void LogicUpdate()
    {

    }

    public byte[] Save() => Array.Empty<byte>();

    public byte[] ClientOnlyData() => Array.Empty<byte>();
}
