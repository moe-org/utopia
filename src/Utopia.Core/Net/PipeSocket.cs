// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Net;

/// <summary>
/// 一个使用<see cref="Pipe"/>实现<see cref="ISocket"/>的类.
/// 通常用于测试.
/// </summary>
public class PipeSocket(PipeReader reader,PipeWriter writer) : ISocket
{
    private bool _disposed = false;

    private readonly PipeReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));

    private readonly PipeWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));

    public bool Alive { get; private set; } = true;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Shutdown();
            Alive = false;
        }

        _disposed = true;
    }

    public EndPoint? RemoteAddress => null;

    public EndPoint? LocalAddress => null;

    public Task<int> Read(Memory<byte> dst)
    {
        if (!Alive)
        {
            return Task.FromResult(0);
        }

        if (_reader.TryRead(out ReadResult result))
        {
            long length = result.Buffer.Length;

            if(length > dst.Length)
            {
                length = dst.Length;
            }

            result.Buffer.Slice(0, length).CopyTo(dst.Span);
            _reader.AdvanceTo(result.Buffer.Slice(0, length).End);

            return Task.FromResult((int)length);
        }

        return Task.FromResult(0);
    }

    public void Shutdown()
    {
        Alive = false;
        _reader.Complete();
        _writer.Complete();
        return;
    }

    public async Task Write(ReadOnlyMemory<byte> data)
    {
        if (!Alive)
        {
            return;
        }

        await _writer.WriteAsync(data);
        await _writer.FlushAsync();
    }

    public static (PipeSocket,PipeSocket) Create()
    {
        Pipe one = new();
        Pipe two = new();

        return new(new(one.Reader, two.Writer), new(two.Reader,one.Writer));
    }
}
