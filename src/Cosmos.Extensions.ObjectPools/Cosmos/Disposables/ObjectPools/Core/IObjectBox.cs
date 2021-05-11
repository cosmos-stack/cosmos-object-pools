using System;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Interface for Object
    /// </summary>
    public interface IObjectBox : IDisposable
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
        DynamicObjectBox GetDynamicObjectOut();

        /// <summary>
        /// Total times acquired<br />
        /// 被获取的总次数
        /// </summary>
        long TotalAcquiredTimes { get; }

        /// <summary>
        /// Time of last acquisition.<br />
        /// 最后一次被获取的时间
        /// </summary>
        DateTime LastAcquiredTime { get; }

        /// <summary>
        /// The time when it was last returned.<br />
        /// 最后归还时的时间
        /// </summary>
        DateTime LastRecycledTime { get; }

        /// <summary>
        /// Created time<br />
        /// 创建时间
        /// </summary>
        DateTime CreatedTime { get; }

        /// <summary>
        /// Thread ID at last acquisition.<br />
        /// 最后获取时的线程 Id
        /// </summary>
        int LastAcquiredThreadId { get; }

        /// <summary>
        /// The thread ID at the time of the last return.<br />
        /// 最后归还时的线程 Id
        /// </summary>
        int LastRecycledThreadId { get; }

        /// <summary>
        /// Reset Value<br />
        /// 重置 Value 值
        /// </summary>
        void Reset();
    }
}