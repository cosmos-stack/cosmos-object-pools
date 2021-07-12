using Cosmos.Disposables.ObjectPools.Core;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for non-generic Object
    /// </summary>
    public interface IObjectCellSite : IObjectCell
    {
        /// <summary>
        /// Resource object.<br />
        /// 资源对象
        /// </summary>
        object Value { get; }
    }
}