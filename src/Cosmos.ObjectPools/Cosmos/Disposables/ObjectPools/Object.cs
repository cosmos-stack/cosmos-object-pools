using System;
using System.Text;
using System.Threading;
using Cosmos.Disposables.ObjectPools.Abstractions;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Recyclable resource objects.<br />
    /// 可回收资源对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Object<T> : IDisposable
    {
        /// <summary>
        /// Use the specified object pool for initialization.<br />
        /// 使用指定对象池进行初始化
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Object<T> InitWith(IObjectPool<T> pool, int id, T value)
        {
            return new Object<T>
            {
                Pool = pool,
                Id = id,
                Value = value,
                LastGetThreadId = Thread.CurrentThread.ManagedThreadId,
                LastGetTime = DateTime.Now
            };
        }

        /// <summary>
        /// Owning object pool<br />
        /// 所属对象池
        /// </summary>
        public IObjectPool<T> Pool { get; internal set; }

        /// <summary>
        /// Unique identifier in the object pool.<br />
        /// 在对象池中的唯一标识
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// Resource object.<br />
        /// 资源对象
        /// </summary>
        public T Value { get; internal set; }

        /// <summary>
        /// Total times acquired
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal long _getTimes;

        /// <summary>
        /// Total times acquired<br />
        /// 被获取的总次数
        /// </summary>
        public long GetTimes => _getTimes;

        /// <summary>
        /// Time of last acquisition.<br />
        /// 最后一次被获取的时间
        /// </summary>
        public DateTime LastGetTime { get; internal set; }

        /// <summary>
        /// The time when it was last returned.<br />
        /// 最后归还时的时间
        /// </summary>
        public DateTime LastReturnTime { get; internal set; }

        /// <summary>
        /// Created time<br />
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; internal set; } = DateTime.Now;

        /// <summary>
        /// Thread ID at last acquisition.<br />
        /// 最后获取时的线程 Id
        /// </summary>
        public int LastGetThreadId { get; internal set; }

        /// <summary>
        /// The thread ID at the time of the last return.<br />
        /// 最后归还时的线程 Id
        /// </summary>
        public int LastReturnThreadId { get; internal set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Value}, ");
            sb.Append($"Times: {GetTimes}, ");
            sb.Append($"ThreadId(R/G): {LastReturnThreadId}/{LastGetThreadId}, ");
            sb.Append($"Time(R/G): {LastReturnTime:yyyy-MM-dd HH:mm:ss:ms}/{LastGetTime:yyyy-MM-dd HH:mm:ss:ms}");

            return sb.ToString();
        }

        /// <summary>
        /// Reset Value<br />
        /// 重置 Value 值
        /// </summary>
        public void ResetValue()
        {
            if (Value != null)
            {
                try
                {
                    Pool.Policy.OnDestroy(Value);
                }
                catch
                {
                    // ignored
                }

                try
                {
                    (Value as IDisposable)?.Dispose();
                }
                catch
                {
                    // ignored
                }
            }

            T value = default;

            try
            {
                value = Pool.Policy.OnCreate();
            }
            catch
            {
                // ignored
            }

            Value = value;
            LastReturnTime = DateTime.Now;
        }

        /// <summary>
        /// Is returned
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal bool _isReturned = false;

        /// <inheritdoc />
        public void Dispose()
        {
            Pool?.Return(this);
        }
    }
}