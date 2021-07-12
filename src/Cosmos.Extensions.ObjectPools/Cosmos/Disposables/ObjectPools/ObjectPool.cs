using System;
using Cosmos.Disposables.ObjectPools.Pools;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Non-generic Object pool<br />
    /// 对象池
    /// </summary>
    public class ObjectPool : ObjectPoolBase<object, IPolicy, ObjectCellSite>, IObjectCellPool
    {
        /// <summary>
        /// Create a new instance of <see cref="ObjectPool{T}"/>.<br />
        /// 创建对象池
        /// </summary>
        /// <param name="bindingType">绑定的类型</param>
        /// <param name="poolSize">池大小</param>
        /// <param name="createObject">池内对象的创建委托</param>
        /// <param name="onGetObject">获取池内对象成功后，进行使用前操作</param>
        public ObjectPool(Type bindingType, int poolSize, Func<object> createObject, Action<ObjectCellSite> onGetObject = null)
            : base(new DefaultPolicy(bindingType) {PoolSize = poolSize, CreateObject = createObject, OnGetObject = onGetObject}) { }

        /// <summary>
        /// Create a new instance of <see cref="ObjectPool{T}"/>.<br />
        /// 创建对象池
        /// </summary>
        /// <param name="policy">策略</param>
        public ObjectPool(IPolicy policy) : base(policy) { }

        /// <inheritdoc />
        internal override ObjectPoolMode Mode => ObjectPoolMode.NonGenericMode;

        /// <inheritdoc />
        protected override Func<int, ObjectCellSite> RecyclableObjectFactory()
        {
            return count => new ObjectCellSite {Pool = this, Id = count + 1};
        }
    }
}