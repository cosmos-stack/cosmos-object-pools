using System;
using System.Text;
using System.Threading;

// ReSharper disable once CheckNamespace
namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Recyclable resource objects.<br />
    /// 可回收资源对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Object<T> : ObjectOut, IObject<T>
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
        public new IObjectPool<T> Pool { get; internal set; }

        /// <inheritdoc />
        public new T Value { get; internal set; }

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

        /// <inheritdoc />
        public override void ResetValue()
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

        /// <inheritdoc />
        public override void Dispose()
        {
            Pool?.Return(this);
        }
    }
}