using System;
using CosmosStack.Disposables.ObjectPools.Pools;

namespace CosmosStack.Disposables.ObjectPools
{
    /// <summary>
    /// Object pool<br />
    /// 对象池
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public class ObjectPool<T> : ObjectPoolBase<T, IPolicy<T>, ObjectPayload<T>>, IObjectPayloadPool<T>
    {
        /// <summary>
        /// Create a new instance of <see cref="ObjectPool{T}"/>.<br />
        /// 创建对象池
        /// </summary>
        /// <param name="poolSize">池大小</param>
        /// <param name="createObject">池内对象的创建委托</param>
        /// <param name="onGetObject">获取池内对象成功后，进行使用前操作</param>
        public ObjectPool(int poolSize, Func<T> createObject, Action<ObjectPayload<T>> onGetObject = null)
            : base(new DefaultPolicy<T> {PoolSize = poolSize, CreateObject = createObject, OnGetObject = onGetObject}) { }

        /// <summary>
        /// Create a new instance of <see cref="ObjectPool{T}"/>.<br />
        /// 创建对象池
        /// </summary>
        /// <param name="policy">策略</param>
        public ObjectPool(IPolicy<T> policy) : base(policy) { }

        /// <inheritdoc />
        internal override ObjectPoolMode Mode => ObjectPoolMode.GenericMode;

        /// <inheritdoc />
        protected override Func<int, ObjectPayload<T>> RecyclableObjectFactory()
        {
            return count => new ObjectPayload<T> {Pool = this, Id = count + 1};
        }
    }
}