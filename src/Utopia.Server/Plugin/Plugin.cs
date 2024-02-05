// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Autofac;
using NLog;
using Utopia.Core;
using Utopia.Core.Collections;
using Utopia.Core.Net;
using Utopia.Core.Net.Packet;
using Utopia.Core.Plugin;
using Utopia.Core.Utilities;
using Utopia.Server.Entity;
using Utopia.Server.Map;
using Utopia.Server.Net;
using Utopia.Server.Plugin.Entity;

namespace Utopia.Server.Plugin;

public class Plugin : PluginForServer
{
    public required ISafeDictionary<Guuid, IWorldFactory> WorldFactories { private get; init; }

    public required ISafeDictionary<long,IWorld> Worlds { private get; init; }

    public required IEntityManager EntityManager { private get; init; }

    public required IInternetMain InternetMain { private get; init; }

    public required ILifetimeScope Container { private get; init; }

    [ContainerBuild]
    public static void SetupContainer(ContainerBuilder builder)
    {
        builder.RegisterType<Generator>();
        builder.RegisterType<WorldFactory>();
        builder.RegisterType<GrassEntity>();
    }

    [LifecycleHandler(PluginLifeCycle.Activated)]
    public void ActivateEventHandler()
    {
        WorldFactories.TryAdd(
                   IDs.WorldType,
                   Container.Resolve<WorldFactory>());

        var factory = new EmptyEntityFactory();
        factory.Entities.TryAdd(ResourcePack.Entity.GrassEntity.ID, Container.Resolve<GrassEntity>());

        // EntityManager.TryAdd(ResourcePack.Entity.GrassEntity.ID,
        //    factory);

        // process query_map packet
        InternetMain.ClientCreatedEvent.Register(
                (e) =>
                {
                    IConnectHandler handler = e.Result!;

                    var container = Container.BeginLifetimeScope((builder) =>
                    {
                        builder.RegisterInstance(handler);
                        builder.RegisterType<QueryBlockPacketHandler>().AsSelf();
                    });

                    handler.SocketDisconnectEvent.Register((_) =>
                    {
                        container.Dispose();
                    });

                    handler.Packetizer.TryAdd(QueryBlockPacketFormatter.PacketTypeId,
                        new QueryBlockPacketFormatter());
                    handler.Packetizer.TryAdd(LoginPacketFormatter.PacketTypeId,
                        new LoginPacketFormatter());
                    handler.Packetizer.TryAdd(BlockInfoPacketFormatter.PacketTypeId,
                        new BlockInfoPacketFormatter());

                    handler.Dispatcher.TryAdd(QueryBlockPacketFormatter.PacketTypeId,
                        container.Resolve<QueryBlockPacketHandler>());
                }
                );

    }

}
