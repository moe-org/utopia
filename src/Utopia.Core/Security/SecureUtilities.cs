// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Text;

namespace Utopia.Core.Security;
public static class SecureUtilities
{

    private const string StringTable = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890abcdefhhijklmnopqrstuvwxyz";

    private static readonly Lazy<RandomNumberGenerator> SecureRandom = new(RandomNumberGenerator.Create, true);

    public static string GenerateSalt()
    {
        return GenerateRandomString(8);
    }

    /// <summary>
    /// Only generate string using A-Z a-z and 0-9.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string GenerateRandomString(int length)
    {
        using var builder = ZString.CreateStringBuilder(true);

        builder.Grow(length);

        var b = new byte[length];
        SecureRandom.Value.GetBytes(b);

        foreach(var c in b)
        {
            builder.Append(StringTable[c % StringTable.Length]);
        }

        return builder.ToString();
    }

    public static string GetSha256String(string value)
    {
        using var sb = ZString.CreateStringBuilder();
        Encoding enc = Encoding.UTF8;
        byte[] result = SHA256.HashData(enc.GetBytes(value));

        foreach (byte b in result)
            sb.Append(b.ToString("x2"));

        return sb.ToString();
    }
}
