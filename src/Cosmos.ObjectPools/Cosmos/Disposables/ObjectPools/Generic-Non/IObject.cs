using Cosmos.Disposables.ObjectPools.Core;

// ReSharper disable once CheckNamespace
namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for non-generic Object
    /// </summary>
    public interface IObject : IObjectOut
    {
        /// <summary>
        /// Resource object.<br />
        /// 资源对象
        /// </summary>
        object Value { get; }
    }
}