using System;
using System.Threading;
using Cosmos.Disposables.ObjectPools.Core;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Non-generic recyclable resource objects.<br />
    /// 非泛型可回收资源对象
    /// </summary>
    public class ObjectCellSite : ObjectCell<object>, IObjectCellSite
    {
        /// <inheritdoc />
        public ObjectCellSite() { }

        internal ObjectCellSite(string internalId, DynamicObjectCell dynamicObjectCell)
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
        public static ObjectCellSite InitWith(IObjectCellPool pool, int id, object value)
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
        public static ObjectCellSite InitWith(IObjectCellPool pool, int id, DynamicObjectCell dynamicObjectCell)
        {
            var ret = new ObjectCellSite
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
        public IObjectCellPool Pool { get; internal set; }

        /// <inheritdoc />
        public override void Reset()
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
            LastRecycledTime = DateTime.Now;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Pool?.Recycle(this);
        }
    }
}