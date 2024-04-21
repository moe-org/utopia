// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using Utopia.Core;
using Utopia.Core.Mnagement;
using Utopia.Core.Net;
using Utopia.Core.Net.Packet;
using Utopia.Core.Security;

namespace Utopia.Server.Net;
public class LoginPacketHandler : IPacketHandler<LoginPacket>
{
    public required ISqlSugarClient Db { get; init; }

    public Task Handle(ConnectionContext ctx, LoginPacket packet)
    {
        var login = (LoginLock)ctx.Items.GetOrAdd(typeof(LoginLock), () =>
        {
            return new LoginLock();
        })!;

        lock (login.Lock)
        {
            if(ctx.User != null)
            {
                return Task.CompletedTask; // ignore login packet
            }
            else
            {
                // search in databse
                var user = Db.Queryable<User>().Where(it => it.Email == packet.Email).ToList();

                // not found or the database is broken
                if(user.Count != 1)
                {
                    ctx.ReportError("get no or too many users with the email, contact with administrator of the server!").Wait();
                    return Task.CompletedTask;
                }

                var u = user.First();

                // check the process to login
                // hash the input
                packet.Password = SecureUtilities.GetSha256String(packet.Password);

                // add salt
                packet.Password = packet.Password + u.Salt;

                // hash again
                packet.Password = SecureUtilities.GetSha256String(packet.Password);

                // if the password is wrong
                if(packet.Password != u.PasswordSha256)
                {
                    ctx.ReportError("failed to login:the password is wrong").Wait();
                    return Task.CompletedTask;
                }

                // or success
                ctx.User = u;
            }
        }

        return Task.CompletedTask;
    }
}

