namespace Utopia.Core.Plugin;

/// <summary>
///     这个Attribute用于构建容器
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ContainerBuildAttribute : Attribute
{
}
