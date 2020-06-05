using System;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Interface for ObjectPool
    /// </summary>
    public interface IObjectPoolCore : IDisposable
    {
        /// <summary>
        /// Is available<br />
        /// 是否可用
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Unavailable exception<br />
        /// 不可用错误
        /// </summary>
        Exception UnavailableException { get; }

        /// <summary>
        /// Unavailable time<br />
        /// 不可用时间
        /// </summary>
        DateTime? UnavailableTime { get; }

        /// <summary>
        /// Set the object pool to be unavailable,
        /// and then Get/GetAsync will report an error,
        /// and at the same time start the background regular check service to restore availability.<br />
        /// 将对象池设置为不可用，后续 Get/GetAsync 均会报错，同时启动后台定时检查服务恢复可用
        /// </summary>
        /// <param name="exception"></param>
        /// <returns>由【可用】变成【不可用】时返回true，否则返回false</returns>
        bool SetUnavailable(Exception exception);

        /// <summary>
        /// Statistics of objects in the object pool.<br />
        /// 统计对象池中的对象
        /// </summary>
        string Statistics { get; }

        /// <summary>
        /// Completely count the objects in the object pool.<br />
        /// 统计对象池中的对象（完整)
        /// </summary>
        string StatisticsFully { get; }
    }
}