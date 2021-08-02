using System;
using System.Threading.Tasks;
using Cosmos.Disposables.ObjectPools.Pools;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for generic ObjectPool
    /// </summary>
    public interface IObjectPayloadPool<T> : IObjectPool<IPolicy<T>>
    {
        /// <summary>
        /// Access to resources.<br />
        /// 获取资源
        /// </summary>
        /// <param name="timeout">超时</param>
        /// <returns></returns>
         ObjectPayload<T> Acquire(TimeSpan? timeout = null);

        /// <summary>
        /// Access to resources async.<br />
        /// 获取资源
        /// </summary>
        /// <returns></returns>
         Task<ObjectPayload<T>> AcquireAsync();

        /// <summary>
        /// Return the resource after use.<br />
        /// 使用完毕后，归还资源
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="isReset">是否重新创建</param>
        void Recycle(ObjectPayload<T> obj, bool isReset = false);
    }
}