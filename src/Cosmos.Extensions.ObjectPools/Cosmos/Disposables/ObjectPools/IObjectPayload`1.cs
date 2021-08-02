using Cosmos.Disposables.ObjectPools.Core;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for Object{T}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectPayload<out T> : IObjectCell
    {
        /// <summary>
        /// Resource object.<br />
        /// 资源对象
        /// </summary>
        T Value { get; }
    }
}