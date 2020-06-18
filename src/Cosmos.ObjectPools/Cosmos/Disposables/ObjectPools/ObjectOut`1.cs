using System;
using System.Threading;
using Cosmos.Disposables.ObjectPools.Core;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Recyclable resource objects.<br />
    /// 可回收资源对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectOut<T> : ObjectOutBase<T>, IObject<T>
    {
        /// <inheritdoc />
        public ObjectOut() { }

        internal ObjectOut(string internalId, DynamicObjectOut dynamicObjectOut)
            : base(internalId, dynamicObjectOut) { }

        #region InitWith

        /// <summary>
        /// Use the specified object pool for initialization.<br />
        /// 使用指定对象池进行初始化
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ObjectOut<T> InitWith(IObjectPool<T> pool, int id, T value)
        {
            return new ObjectOut<T>
            {
                Pool = pool,
                Id = id,
                Value = value,
                LastGetThreadId = Thread.CurrentThread.ManagedThreadId,
                LastGetTime = DateTime.Now
            };
        }

        /// <summary>
        /// Use the specified object pool for initialization.<br />
        /// 使用指定对象池进行初始化
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="id"></param>
        /// <param name="dynamicObjectOut"></param>
        /// <returns></returns>
        public static ObjectOut<T> InitWith(IObjectPool<T> pool, int id, DynamicObjectOut dynamicObjectOut)
        {
            var ret = new ObjectOut<T>
            {
                Pool = pool,
                Id = id,
                LastGetThreadId = Thread.CurrentThread.ManagedThreadId,
                LastGetTime = DateTime.Now
            };

            ret.SetDynamicObjectOut(dynamicObjectOut);

            return ret;
        }

        #endregion

        /// <summary>
        /// Owning object pool<br />
        /// 所属对象池
        /// </summary>
        public IObjectPool<T> Pool { get; internal set; }

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

        // /// <summary>
        // /// Implicit convert ObjectOut`1 to ObjectOut
        // /// </summary>
        // /// <param name="that"></param>
        // /// <returns></returns>
        // public static implicit operator ObjectOut(ObjectOut<T> that)
        // {
        //     var ret = new ObjectOut(that.InternalIdentity, that.GetDynamicObjectOut())
        //     {
        //         Id = that.Id,
        //         Pool = that.Pool,
        //         CreateTime = that.CreateTime,
        //         LastGetTime = that.LastGetTime,
        //         LastGetThreadId = that.LastGetThreadId,
        //         LastReturnTime = that.LastReturnTime,
        //         LastReturnThreadId = that.LastReturnThreadId,
        //         _getTimes = that._getTimes,
        //         _isReturned = that._isReturned,
        //     };
        //
        //     return ret;
        // }
        //
        // /// <summary>
        // /// Implicit convert ObjectOut to ObjectOut`1
        // /// </summary>
        // /// <param name="that"></param>
        // /// <returns></returns>
        // public static implicit operator ObjectOut<T>(ObjectOut that)
        // {
        //     var ret = new ObjectOut<T>(that.InternalIdentity, that.GetDynamicObjectOut())
        //     {
        //         Id = that.Id,
        //         Pool = that.Pool,
        //         CreateTime = that.CreateTime,
        //         LastGetTime = that.LastGetTime,
        //         LastGetThreadId = that.LastGetThreadId,
        //         LastReturnTime = that.LastReturnTime,
        //         LastReturnThreadId = that.LastReturnThreadId,
        //         _getTimes = that._getTimes,
        //         _isReturned = that._isReturned,
        //     };
        //
        //     return ret;
        // }
    }
}