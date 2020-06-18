using System;
using System.Threading;
using Cosmos.Disposables.ObjectPools.Core;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Non-generic recyclable resource objects.<br />
    /// 非泛型可回收资源对象
    /// </summary>
    public class ObjectOut : ObjectOutBase<object>, IObject
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
        public static ObjectOut InitWith(IObjectPool pool, int id, object value)
        {
            return new ObjectOut
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
        public static ObjectOut InitWith(IObjectPool pool, int id, DynamicObjectOut dynamicObjectOut)
        {
            var ret = new ObjectOut
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
        public IObjectPool Pool { get; internal set; }

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

            object value = default;

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