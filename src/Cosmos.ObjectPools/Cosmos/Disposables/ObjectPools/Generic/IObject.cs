using Cosmos.Disposables.ObjectPools.Core;

// ReSharper disable once CheckNamespace
namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for Object{T}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObject<out T> : IObjectOut
    {
        /// <summary>
        /// Resource object.<br />
        /// 资源对象
        /// </summary>
        T Value { get; }
    }
}