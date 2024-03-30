// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Net;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using SqlSugar;

namespace Utopia.Server;

public static class Program
{
    /// <summary>
    /// the ascii logo
    /// </summary>
    public const string Art =
        """
                           ___
                 ,--,    ,--.'|_              ,-.----.     ,--,
               ,'_ /|    |  | :,'     ,---.   \\    /  \\  ,--.'|
          .--. |  | :    :  : ' :    '   ,'\\  |   :    | |  |,
        ,'_ /| :  . |  .;__,'  /    /   /   | |   | .\\ : `--'_        ,--.--.
        |  ' | |  . .  |  |   |    .   ; ,. : .   : |: | ,' ,'|      /       \\
        |  | ' |  | |  :__,'| :    '   | |: : |   |  \\ : '  | |     .--.  .-. |
        :  | | :  ' ;    '  : |__  '   | .; : |   : .  | |  | :      \\__\\/: . .
        |  ; ' |  | '    |  | '.'| |   :    | :     |`-' '  : |__    ," .--.; |
        :  | : ;  ; |    ;  :    ;  \\   \\  /  :   : :    |  | '.'|  /  /  ,.  |
        '  :  `--'   \\   |  ,   /    `----'   |   | :    ;  :    ; ;  :   .'   \\
        :  ,      .-./    ---`-'              `---'.|    |  ,   /  |  ,     .-./
         `--`----'                              `---`     ---`-'    `--`---'
        """;

    public static void CheckAndPrintArt()
    {
        try
        {
            if (Console.BufferWidth >= Art.Split('\n').MaxBy((s) => s.Length)!.Length)
            {
                Console.WriteLine(Art);
            }
        }
        catch
        {
            // if we can not print the logo,it does not matter
        }
    }

    public static ISqlSugarClient ConnectToSqlite(string connectString)
    {
        return new SqlSugarClient(new ConnectionConfig()
        {
            ConnectionString = connectString,//连接符字串
            DbType = DbType.Sqlite,//数据库类型
            IsAutoCloseConnection = true //不设成true要手动close
        },
        db =>
        {
            //(A)全局生效配置点，一般AOP和程序启动的配置扔这里面 ，所有上下文生效
            //调试SQL事件，可以删掉
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                //获取原生SQL推荐 5.1.4.63  性能OK
                // Console.WriteLine(UtilMethods.GetNativeSql(sql, pars));

                //获取无参数化SQL 对性能有影响，特别大的SQL参数多的，调试使用
                //Console.WriteLine(UtilMethods.GetSqlString(DbType.SqlServer,sql,pars))
            };
        });
    }

    /// <summary>
    /// 这是默认的启动程序。
    /// 可以自行编写其他的启动程序。
    /// </summary>
    /// <param name="args">命令行参数只是为了方便，实际上命令行参数无法覆盖所有设置！</param>
    public static void Main(string[] args)
    {
        // connect to database
        Func<ISqlSugarClient> scope;
        if (args.Length >= 1 && args[0].StartsWith("Sqlite:"))
        {
            scope = () => ConnectToSqlite(args[0][("Sqlite:".Length)..]);
        }
        else
        {
            scope = () => ConnectToSqlite("file::memory:");
            Console.WriteLine("You are using a in-memory database(sqlite with in-memory mode) now!");
            Console.WriteLine("You will lose all the data once the server shutdown.");
            Console.WriteLine(
                "use `Sqlite:Your Connection String` " +
                "as the first argument " +
                "to connect to the database.");
        }

        var opt = Launcher.LauncherOption.ParseOptions(args);

        var launcher = new Launcher(opt);

        launcher.Builder!
            .Register((t) =>
        {
            return scope.Invoke();
        })
            .As<ISqlSugarClient>()
            .InstancePerDependency();

        launcher.Builder!.RegisterInstance(scope).As<ISqlSugarClient>();

        launcher.Launch();

        launcher.MainTask!.Wait();
    }
}
