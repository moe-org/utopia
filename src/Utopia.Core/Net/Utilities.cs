#region

using System.Net;
using System.Net.NetworkInformation;
using Autofac;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Utopia.Core.Net.Middlewares;
using Utopia.Core.Net.Packet;

#endregion

namespace Utopia.Core.Net;

public static class Utilities
{
    public static async Task<PingReply?> TryPing(
        IPAddress address,
        int timeoutSeconds = 1,
        int retry = 3)
    {
        var tried = 0;

        while (tried < retry)
        {
            // try ping
            tried++;
            try
            {
                Ping pingTest = new();
                var reply = await pingTest.SendPingAsync(
                    address,
                    new TimeSpan(0, 0, timeoutSeconds));

                if (reply.Status != IPStatus.Success) continue;

                return reply;
            }
            catch
            {
            }
        }

        return null;
    }

    public static int GetMtu()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var mtu = 1500;
            long maxByteOut = 0;

            foreach (var @interface in interfaces)
                // 使用发送数据量最大的interface的MTU.
                // 这通常应该有用.
                if (@interface.GetIPv4Statistics().BytesSent > maxByteOut)
                {
                    maxByteOut = @interface.GetIPv4Statistics().BytesSent;
                    mtu = @interface.GetIPProperties().GetIPv4Properties().Mtu;
                }

            return mtu;
        }
        catch
        {
            return 1500;
        }
    }

    public static IConnectionBuilder UseMiddleware(this IConnectionBuilder builder, IMiddleware middleware)
    {
        builder.Use(async (context, next) =>
        {
            if(context is KestrelConnectionContext uContext)
            {
                await middleware.InvokeAsync(uContext, async (nContext) =>
                {
                    await next(nContext.Connection);
                });
            }
        });

        return builder;
    }

    public static IConnectionBuilder EnableMiddleware(this IConnectionBuilder builder, IComponentContext container)
    {
        var adaptor = new KestrelInitlizeRawMiddleware() { LifetimeScope = container.Resolve<ILifetimeScope>() };

        builder.Use(adaptor.InvokeAsync);

        return builder;
    }

    public static async Task ReportError(this ConnectionContext ctx,string msg)
    {
        await ctx.PacketWriter.WriteAsync(new ParsedPacket(ErrorPacket.PacketID, new ErrorPacket() { ErrorMessage = msg }), ctx.ConnectionClosed);
    }
}
