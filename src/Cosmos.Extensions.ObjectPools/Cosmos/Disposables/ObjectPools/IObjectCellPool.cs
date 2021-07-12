using System;
using System.Threading.Tasks;
using Cosmos.Disposables.ObjectPools.Pools;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for non-generic ObjectPool
    /// </summary>
    public interface IObjectCellPool : IObjectPool<IPolicy>
    {
        /// <summary>
        /// Access to resources.<br />
        /// 获取资源
        /// </summary>
        /// <param name="timeout">超时</param>
        /// <returns></returns>
        ObjectCellSite Acquire(TimeSpan? timeout = null);

        /// <summary>
        /// Access to resources async.<br />
        /// 获取资源
        /// </summary>
        /// <returns></returns>
        Task<ObjectCellSite> AcquireAsync();

        /// <summary>
        /// Return the resource after use.<br />
        /// 使用完毕后，归还资源
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="isReset">是否重新创建</param>
        void Recycle(ObjectCellSite obj, bool isReset = false);
    }
}