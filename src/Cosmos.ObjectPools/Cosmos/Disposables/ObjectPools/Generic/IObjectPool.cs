using System;
using System.Threading.Tasks;
using Cosmos.Disposables.ObjectPools.Core;

// ReSharper disable once CheckNamespace
namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for generic ObjectPool
    /// </summary>
    public interface IObjectPool<T> : IObjectPool
    {
        /// <summary>
        /// Gets policy
        /// </summary>
        new IPolicy<T> Policy { get; }

        /// <summary>
        /// Access to resources.<br />
        /// 获取资源
        /// </summary>
        /// <param name="timeout">超时</param>
        /// <returns></returns>
        new Object<T> Get(TimeSpan? timeout = null);

        /// <summary>
        /// Access to resources async.<br />
        /// 获取资源
        /// </summary>
        /// <returns></returns>
        new Task<Object<T>> GetAsync();

        /// <summary>
        /// Return the resource after use.<br />
        /// 使用完毕后，归还资源
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="isReset">是否重新创建</param>
        void Return(Object<T> obj, bool isReset = false);
    }
}