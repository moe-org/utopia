#region

using System.Buffers;
using System.IO.Pipelines;
using System.Net;

#endregion

namespace Utopia.Core.Net;

/// <summary>
///     一个使用<see cref="Pipe" />实现<see cref="ISocket" />的类.
///     通常用于测试.
/// </summary>
public class PipeSocket(PipeReader reader, PipeWriter writer) : ISocket
{
    private readonly PipeReader _reader = reader ?? throw new ArgumentNullException(nameof(reader));

    private readonly PipeWriter _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    private bool _disposed;

    public bool Alive { get; private set; } = true;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public EndPoint? RemoteAddress => null;

    public EndPoint? LocalAddress => null;

    public Task<int> Read(Memory<byte> dst)
    {
        if (!this.Alive) return Task.FromResult(0);

        if (this._reader.TryRead(out var result))
        {
            var length = result.Buffer.Length;

            if (length > dst.Length) length = dst.Length;

            result.Buffer.Slice(0, length).CopyTo(dst.Span);
            this._reader.AdvanceTo(result.Buffer.Slice(0, length).End);

            return Task.FromResult((int)length);
        }

        return Task.FromResult(0);
    }

    public void Shutdown()
    {
        this.Alive = false;
        this._reader.Complete();
        this._writer.Complete();
    }

    public async Task Write(ReadOnlyMemory<byte> data)
    {
        if (!this.Alive) return;

        await this._writer.WriteAsync(data);
        await this._writer.FlushAsync();
    }

    protected void Dispose(bool disposing)
    {
        if (this._disposed) return;

        if (disposing)
        {
            this.Shutdown();
            this.Alive = false;
        }

        this._disposed = true;
    }

    public static (PipeSocket, PipeSocket) Create()
    {
        Pipe one = new();
        Pipe two = new();

        return new ValueTuple<PipeSocket, PipeSocket>(new PipeSocket(one.Reader, two.Writer),
            new PipeSocket(two.Reader, one.Writer));
    }
}