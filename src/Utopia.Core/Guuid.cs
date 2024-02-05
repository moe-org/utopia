#region

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using Cysharp.Text;
using MemoryPack;
using Utopia.Core.Exceptions;

#endregion

namespace Utopia.Core;

/// <summary>
///     globally unique universally identifier.
///     This is a constant class(like string).
///     人类可读的唯一标识符。由name组成。至少必须要存在有两个name。
///     第一个name称为root。其余的all name称为node(s)。
///     name的长度不能为0，只能由字母、数字组成。
///     并且只能由字母开头。
///     guuid的字符串形式类似于：root:namespaces1:namespaces2...
/// </summary>
[MemoryPackable]
public readonly partial struct Guuid : IEnumerable<string>
{
    /// <summary>
    ///     Use this regex pattern to check the guuid string
    /// </summary>
    public const string Pattern = @"^[a-zA-Z]{1}[a-zA-Z0-9]*(\.[a-zA-Z]{1}[a-zA-Z0-9]*)+$";

    /// <summary>
    ///     utopia游戏所使用的GUUID的root.
    /// </summary>
    public const string UtopiaRoot = "Utopia";

    /// <summary>
    ///     Will use this to separate root and each namespaces when use the string of Guuid.
    /// </summary>
    public const string Separator = ".";

    public static readonly Guuid Empty = new("Empty", "Empty");

    /// <summary>
    ///     获取一个内部的Guuid，使用<see cref="UtopiaRoot" />作为Root
    /// </summary>
    internal static Guuid GetInternalGuuid(params string[] nodes)
    {
        return new Guuid(UtopiaRoot, nodes);
    }

    /// <summary>
    ///     检查name是否符合要求
    /// </summary>
    /// <param name="name">要检查的name</param>
    /// <returns>如果name合法，返回true。</returns>
    public static bool CheckName(string? name)
    {
        return !string.IsNullOrEmpty(name)
               && char.IsLetter(name.First()) && name.All(c => char.IsLetter(c) || char.IsDigit(c));
    }

    /// <summary>
    ///     检查整个guuid是否符合要求。
    /// </summary>
    /// <param name="guuid">guuid字符串</param>
    /// <returns>如果符合要求，则返回true，否则返回false</returns>
    public static bool CheckGuuid(string guuid)
    {
        ArgumentNullException.ThrowIfNull(guuid);
        if (string.IsNullOrEmpty(guuid)) return false;

        var nodes = guuid.Split(Separator);

        // 至少要存在一个root和一个node
        return nodes.Length >= 2 && CheckGuuid(nodes.First(), nodes[1..]);
    }

    /// <summary>
    ///     检查guuid是否符合要求
    /// </summary>
    /// <param name="root">guuid的root</param>
    /// <param name="nodes">guuid的节点</param>
    /// <returns>如果符合要求，返回true，否则返回false。</returns>
    public static bool CheckGuuid(string? root, params string?[]? nodes)
    {
        if (root is null) return false;
        if (nodes is null) return false;

        if (!CheckName(root)) return false;
        foreach (var node in nodes)
            if (!CheckName(node))
                return false;

        return true;
    }

    /// <exception cref="ArgumentException">如果root或者nodes不符合规范则抛出</exception>
    [MemoryPackConstructor]
    public Guuid(string root, params string[] nodes)
    {
        if (!CheckGuuid(root, nodes)) throw new ArgumentException("the guuid name is illegal");

        this.Root = root;
        this.Nodes = nodes;
    }

    public string Root { get; init; }

    public string[] Nodes { get; init; }

    public static bool operator ==(Guuid c1, Guuid c2)
    {
        return c1.Root == c2.Root && c1.Nodes.SequenceEqual(c2.Nodes);
    }

    public static bool operator !=(Guuid c1, Guuid c2)
    {
        return c1.Root != c2.Root || !c1.Nodes.SequenceEqual(c2.Nodes);
    }

    /// <summary>
    ///     把guuid转换为字符串形式. Will use <see cref="Separator" /> to separate root and each namespaces.
    ///     For example,
    ///     a guuid with root `r` and namespaces `a` and `b` will have a string form as `r:a:b`
    ///     (If <see cref="Separator" /> is `.`)
    /// </summary>
    public override string ToString()
    {
        using var builder = ZString.CreateStringBuilder(true);
    
        builder.Append(this.Root);
        foreach (var node in this.Nodes)
        {
#pragma warning disable CA1834 // Consider using 'StringBuilder.Append(char)' when applicable
            builder.Append(Separator);
            builder.Append(node);
#pragma warning restore CA1834 // Consider using 'StringBuilder.Append(char)' when applicable
        }

        return builder.ToString();
    }

    /// <summary>
    ///     从字符串解析Guuid
    /// </summary>
    /// <param name="s">字符串应该是来自Guuid的ToString()的结果。</param>
    /// <exception cref="ArgumentException">输入的字符串有误</exception>
    public static Guuid Parse(string s)
    {
        return !TryParse(s, out var result, out var msg) 
            ? throw GuuidFormatException.Throw(s, msg) 
            : result.Value;
    }

    /// <summary>
    ///     See <see cref="Parse(string)" />
    /// </summary>
    /// <param name="s"></param>
    /// <param name="result"></param>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool TryParse(
        string s,
        [NotNullWhen(true)] out Guuid? result,
        [NotNullWhen(false)] out string? errorMessage)
    {
        if (string.IsNullOrEmpty(s)) throw new ArgumentException("param is empty or null");
        result = null;
        errorMessage = null;

        var nodes = s.Split(Separator);

        if (nodes.Length < 2)
        {
            errorMessage =
                "the guuid format is illegal.(get too less substring from Split(),check the separator is right)";
            return false;
        }

        foreach (var item in nodes)
            if (!CheckName(item))
            {
                errorMessage = $"the name of guuid '{item}' is invalid.";
                return false;
            }

        result = new Guuid(nodes.First(), nodes[1..]);
        return true;
    }

    /// <summary>
    ///     获取一个新的随机的标识符。
    /// </summary>
    public static Guuid Unique()
    {
        var rno = RandomNumberGenerator.GetBytes(16);
        var high = BitConverter.ToUInt64(rno, 0);
        var low = BitConverter.ToUInt64(rno, 8);

        return new Guuid("Unique", $"{high:X16}{low:X16}");
    }

    public override bool Equals(object? obj)
    {
        return obj is not null && obj is Guuid guuid && this == guuid;
    }

    public override int GetHashCode()
    {
        var hasher = new XxHash32(0);
        hasher.Append(Encoding.UTF8.GetBytes(this.Root));

        foreach (var node in this.Nodes) hasher.Append(Encoding.UTF8.GetBytes(node));

        var bytes = hasher.GetHashAndReset();
        return BitConverter.ToInt32(bytes);
    }

    public Guuid Append(Guuid guuid)
    {
        var root = this.Root;
        var nodes = this.Nodes.ToList();
        nodes.Capacity = nodes.Count + guuid.Nodes.Length + 1;
        nodes.Add(guuid.Root);
        nodes.AddRange(guuid.Nodes);
        return new Guuid(root, nodes.ToArray());
    }

    public Guuid Append(params string[] otherNodes)
    {
        var root = this.Root;
        var nodes = this.Nodes.ToList();
        nodes.Capacity = nodes.Count + otherNodes.Length;
        nodes.AddRange(otherNodes);
        return new Guuid(root, nodes.ToArray());
    }

    /// <summary>
    ///     Cover this guuid to C# identifier
    /// </summary>
    /// <returns></returns>
    public string ToCsIdentifier()
    {
        return this.Nodes.Aggregate('@' + this.Root, (result, value) => result + "_" + value);
    }

    public IEnumerator<string> GetEnumerator()
    {
        yield return this.Root;
        foreach (var s in this.Nodes) yield return s;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    /// <summary>
    ///     Check if another guuid is the child of this guuid.
    ///     e.g. `a:b:c:d` is a child guuid of `a:b:c:d` or `a:b:c` or `a:b`.
    /// </summary>
    /// <returns>true if another guuid is the child of this.</returns>
    public bool HasChild(in Guuid id)
    {
        using var node = id.GetEnumerator();
        node.MoveNext();
        foreach (var item in this)
        {
            if (node.Current != item) return false;

            // the child is short than(or equal in length) father!!!
            if (!node.MoveNext())
                // check if two guuid are the same
                return id == this;
        }

        return true;
    }

    /// <summary>
    ///     Get an root of the guuid
    /// </summary>
    /// <returns>如果这个Guuid只剩下一个node，那么返回null。</returns>
    public Guuid? GetParent()
    {
        if (this.Nodes.Length == 1) return null;

        return new Guuid(this.Root, this.Nodes[..^1]);
    }
}