using System;
using System.Threading;
using Cosmos.Disposables.ObjectPools.Core;
using Cosmos.Exceptions;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Recyclable resource objects.<br />
    /// 可回收资源对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectBox<T> : ObjectBoxBase<T>, IObject<T>
    {
        /// <inheritdoc />
        public ObjectBox() { }

        internal ObjectBox(string internalId, DynamicObjectBox dynamicObjectBox)
            : base(internalId, dynamicObjectBox) { }

        #region InitWith

        /// <summary>
        /// Use the specified object pool for initialization.<br />
        /// 使用指定对象池进行初始化
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ObjectBox<T> InitWith(IObjectPool<T> pool, int id, T value)
        {
            return new()
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
        /// <param name="dynamicObjectBox"></param>
        /// <returns></returns>
        public static ObjectBox<T> InitWith(IObjectPool<T> pool, int id, DynamicObjectBox dynamicObjectBox)
        {
            var ret = new ObjectBox<T>
            {
                Pool = pool,
                Id = id,
                LastGetThreadId = Thread.CurrentThread.ManagedThreadId,
                LastGetTime = DateTime.Now
            };

            ret.SetDynamicObjectOut(dynamicObjectBox);

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
            if (Value is not null)
            {
                Try.Invoke(() => Pool.Policy.OnDestroy(Value));
                Try.Invoke(() => (Value as IDisposable)?.Dispose());
            }

            Value = Try.Create(Pool.Policy.OnCreate).GetSafeValue(default(T));
            LastReturnTime = DateTime.Now;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Pool?.Return(this);
        }

        // /// <summary>
        // /// Implicit convert ObjectBox`1 to ObjectOut
        // /// </summary>
        // /// <param name="that"></param>
        // /// <returns></returns>
        // public static implicit operator ObjectBox(ObjectBox<T> that)
        // {
        //     var ret = new ObjectBox(that.InternalIdentity, that.GetDynamicObjectOut())
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
        // /// Implicit convert ObjectBox to ObjectOut`1
        // /// </summary>
        // /// <param name="that"></param>
        // /// <returns></returns>
        // public static implicit operator ObjectBox<T>(ObjectBox that)
        // {
        //     var ret = new ObjectBox<T>(that.InternalIdentity, that.GetDynamicObjectOut())
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