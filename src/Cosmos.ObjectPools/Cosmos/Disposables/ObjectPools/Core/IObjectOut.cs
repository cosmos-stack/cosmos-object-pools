using System;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Interface for Object
    /// </summary>
    public interface IObjectOut : IDisposable
    {
        /// <summary>
        /// Unique identifier in the object pool.<br />
        /// 在对象池中的唯一标识
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Get a copy of DynamicObjectOut instance.
        /// </summary>
        /// <returns></returns>
        DynamicObjectOut GetDynamicObjectOut();

        /// <summary>
        /// Total times acquired<br />
        /// 被获取的总次数
        /// </summary>
        long GetTimes { get; }

        /// <summary>
        /// Time of last acquisition.<br />
        /// 最后一次被获取的时间
        /// </summary>
        DateTime LastGetTime { get; }

        /// <summary>
        /// The time when it was last returned.<br />
        /// 最后归还时的时间
        /// </summary>
        DateTime LastReturnTime { get; }

        /// <summary>
        /// Created time<br />
        /// 创建时间
        /// </summary>
        DateTime CreateTime { get; }

        /// <summary>
        /// Thread ID at last acquisition.<br />
        /// 最后获取时的线程 Id
        /// </summary>
        int LastGetThreadId { get; }

        /// <summary>
        /// The thread ID at the time of the last return.<br />
        /// 最后归还时的线程 Id
        /// </summary>
        int LastReturnThreadId { get; }

        /// <summary>
        /// Reset Value<br />
        /// 重置 Value 值
        /// </summary>
        void ResetValue();
    }
}