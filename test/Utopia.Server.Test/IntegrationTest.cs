// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using Utopia.Core;
using Autofac;
using SqlSugar;
using Utopia.Server.Net;
using System.Net.NetworkInformation;

namespace Utopia.Server.Test;

public abstract class IntegrationTest : IDisposable
{
    /// <summary>
    /// just access <see cref="Launcher"/> and other fields to get or set option.
    /// </summary>
    protected abstract void BeforeRun();

    protected abstract void AfterRun();

    /// <summary>
    /// just access <see cref="Launcher"/> and other fields to get or set option.
    /// </summary>

    protected abstract void Configure();

    private bool _disposed = false;

    protected void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            StopAndWait();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected TestableLauncher Launcher { get; set; }

    protected Launcher.Options Options { get; set; }

    protected void StopAndWait()
    {
        Launcher.Container?.Resolve<MainThread>()?.GracefulStop.TrySetResult();
        try { Launcher.MainTask?.Wait(); }
        catch (Exception) { }
    }

    protected static readonly object _lock = new();
    protected static readonly HashSet<int> UsedPorts = [];

    protected int GetNextFreePort()
    {
        bool isAvailable(in int toCheck)
        {
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == toCheck)
                {
                    isAvailable = false;
                    break;
                }
            }

            return isAvailable;
        }

        lock (_lock)
        {
            int port = Random.Shared.Next(1024, 8192);

            while (UsedPorts.Contains(port) || !isAvailable(port))
            {
                UsedPorts.Add(port);
                port = Random.Shared.Next(1024, 8192);
            }

            UsedPorts.Add(port);

            return port;
        }
    }

    public IntegrationTest(ITestOutputHelper logger)
    {
        Options = TestableLauncher.GetDefaultTestOption();

        Options.Port = GetNextFreePort();

        Configure();

        Launcher = new(Options, logger);

        Launcher.UseKestrelForServer();

        Launcher.Builder!
            .Register((t) =>
            {
                return Program.ConnectToSqlite(":memory:");
            })
            .As<ISqlSugarClient>()
            .InstancePerDependency();

        // run
        BeforeRun();

        Launcher.Launch();

        AfterRun();

        // wait for start
        Launcher.Container!.Resolve<MainThread>().StartTask.Wait();
    }
}
