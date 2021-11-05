using System;
using System.Threading;
using CosmosStack.Disposables.ObjectPools.Core;
using CosmosStack.Exceptions;

namespace CosmosStack.Disposables.ObjectPools
{
    /// <summary>
    /// Recyclable resource objects.<br />
    /// 可回收资源对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPayload<T> : ObjectCell<T>, IObjectPayload<T>
    {
        /// <inheritdoc />
        public ObjectPayload() { }

        internal ObjectPayload(string internalId, DynamicObjectCell dynamicObjectCell)
            : base(internalId, dynamicObjectCell) { }

        #region InitWith

        /// <summary>
        /// Use the specified object pool for initialization.<br />
        /// 使用指定对象池进行初始化
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ObjectPayload<T> InitWith(IObjectPayloadPool<T> pool, int id, T value)
        {
            return new()
            {
                Pool = pool,
                Id = id,
                Value = value,
                LastAcquiredThreadId = Thread.CurrentThread.ManagedThreadId,
                LastAcquiredTime = DateTime.Now
            };
        }

        /// <summary>
        /// Use the specified object pool for initialization.<br />
        /// 使用指定对象池进行初始化
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="id"></param>
        /// <param name="dynamicObjectCell"></param>
        /// <returns></returns>
        public static ObjectPayload<T> InitWith(IObjectPayloadPool<T> pool, int id, DynamicObjectCell dynamicObjectCell)
        {
            var ret = new ObjectPayload<T>
            {
                Pool = pool,
                Id = id,
                LastAcquiredThreadId = Thread.CurrentThread.ManagedThreadId,
                LastAcquiredTime = DateTime.Now
            };

            ret.SetDynamicObjectOut(dynamicObjectCell);

            return ret;
        }

        #endregion

        /// <summary>
        /// Owning object pool<br />
        /// 所属对象池
        /// </summary>
        public IObjectPayloadPool<T> Pool { get; internal set; }

        /// <inheritdoc />
        public override void Reset()
        {
            if (Value is not null)
            {
                Try.Invoke(() => Pool.Policy.OnDestroy(Value));
                Try.Invoke(() => (Value as IDisposable)?.Dispose());
            }

            Value = Try.Create(Pool.Policy.OnCreate).GetSafeValue(default(T));
            LastRecycledTime = DateTime.Now;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Pool?.Recycle(this);
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