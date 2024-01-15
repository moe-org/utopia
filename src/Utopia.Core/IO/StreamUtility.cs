// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Net;
using System.Text;
using CommunityToolkit.Diagnostics;

namespace Utopia.Core.IO;

/// <summary>
/// socket工具
/// </summary>
public static class StreamUtility
{

    /// <summary>
    /// 将会调用<see cref="IPAddress.NetworkToHostOrder(int)"/>进行处理
    /// </summary>
    /// <param name="socket"></param>
    /// <returns></returns>
    public static async Task<int> ReadIntWithEndianConver(Stream stream) => IPAddress.NetworkToHostOrder(await ReadInt(stream));

    public static async Task<int> ReadInt(Stream stream)
    {
        byte[] buffer = await Read(stream, 4);

        return BitConverter.ToInt32(buffer);

    }

    public static async Task<byte[]> Read(Stream stream, int length)
    {
        Guard.IsNotNull(stream);

        byte[] buffer = new byte[length];
        int ptr = 0;
        while (ptr != length)
        {
            ptr += await stream.ReadAsync(new ArraySegment<byte>(buffer, ptr, buffer.Length - ptr));
        }

        return buffer;
    }

    /// <summary>
    /// It will read a length then read the data
    /// 将调用<see cref="ReadIntWithEndianConver"/>,
    /// 即<see cref="IPAddress.NetworkToHostOrder"/>
    /// </summary>
    /// <returns></returns>
    public static async Task<byte[]> ReadPacket(Stream stream)
    {
        Guard.IsNotNull(stream);

        int l = await ReadIntWithEndianConver(stream);
        return await Read(stream, l);
    }

    /// <summary>
    /// 将会进行端序转换.
    /// 即调用<see cref="ReadIntWithEndianConver"/>
    /// </summary>
    public static async Task<string> ReadString(Stream stream)
    {
        Guard.IsNotNull(stream);

        int l = await ReadIntWithEndianConver(stream);
        return Encoding.UTF8.GetString(await Read(stream, l));
    }

    /// <summary>
    /// 将会进行端序转换.
    /// 即调用<see cref="ReadIntWithEndianConver"/>
    /// </summary>
    public static async Task<Guuid> ReadGuuid(Stream stream) => Guuid.Parse(await ReadString(stream));

    /// <summary>
    /// This will call <see cref="IPAddress.HostToNetworkOrder(int)"/> method to covert the value.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static async Task WriteInt(Stream stream, int value)
    {
        byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));

        await stream.WriteAsync(bytes);
    }

    /// <summary>
    /// will call <see cref="WriteInt"/> to write length.
    /// </summary>
    public static async Task WriteStringWithLength(Stream stream, string value)
    {
        await WriteInt(stream, value.Length);

        await stream.WriteAsync(Encoding.UTF8.GetBytes(value));

    }

    /// <summary>
    /// will call <see cref="WriteInt"/> to write length.
    /// </summary>
    public static async Task WriteDataWithLength(Stream stream, byte[] data)
    {
        await WriteInt(stream, data.Length);

        await stream.WriteAsync(data);
    }
}

