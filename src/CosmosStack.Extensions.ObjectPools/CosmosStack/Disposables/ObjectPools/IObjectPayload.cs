using CosmosStack.Disposables.ObjectPools.Core;

namespace CosmosStack.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for non-generic Object
    /// </summary>
    public interface IObjectPayload : IObjectCell
    {
        /// <summary>
        /// Resource object.<br />
        /// 资源对象
        /// </summary>
        object Value { get; }
    }
}