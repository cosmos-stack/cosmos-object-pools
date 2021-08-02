using System;
using Cosmos.Disposables.ObjectPools.Pools;

// ReSharper disable InconsistentNaming

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Object pool manager
    /// </summary>
    public static partial class ObjectPoolManager
    {
        internal const string DefaultName = "__default";
        private static readonly IObjectPoolManagedModel _defaultManagedModel;

        static ObjectPoolManager()
        {
            _defaultManagedModel = new ObjectPoolManagedModel();
        }

        #region Get

        /// <summary>
        /// To get the specified type of object pool.<br />
        /// 获取指定类型的对象池。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"> Unknown type.</exception>
        /// <exception cref="ArgumentException">Unable to get the specified type of object pool.</exception>
        public static IObjectPayloadPool<T> Get<T>()
        {
            return _defaultManagedModel.GetDefaultTyped<T>();
        }

        /// <summary>
        /// To get the specified type and name of object pool.<br />
        /// 获取指定类型和名称的对象池。
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"> Unknown type or name.</exception>
        /// <exception cref="ArgumentException">Unable to get the specified type and name of object pool.</exception>
        public static IObjectPayloadPool<T> Get<T>(string name)
        {
            return _defaultManagedModel.Get<T>(name);
        }

        /// <summary>
        /// To get the specified type of object pool.<br />
        /// 获取指定类型的对象池。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IObjectPayloadPool Get(Type type)
        {
            return _defaultManagedModel.GetDefaultTyped(type);
        }

        /// <summary>
        /// To get the specified type and name of object pool.<br />
        /// 获取指定类型和名称的对象池。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"> Unknown type or name.</exception>
        /// <exception cref="ArgumentException">Unable to get the specified type and name of object pool.</exception>
        public static IObjectPayloadPool Get(Type type, string name)
        {
            return _defaultManagedModel.Get(type, name);
        }

        #endregion

        #region Create

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObjectPayloadPool<T> Create<T>(int poolSize, Func<T> createObjectFunc, Action<ObjectPayload<T>> getObjectHandler = null)
        {
            if (Contains<T>())
                throw new ArgumentException("The specified type of object pool is exist.");

            var type = typeof(T);
            var pool = new ObjectPool<T>(poolSize, createObjectFunc, getObjectHandler);

            UpdateObjectPools(type, DefaultName, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool<T> Create<T>(string name, int poolSize, Func<T> createObjectFunc, Action<ObjectPayload<T>> getObjectHandler = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = DefaultName;

            if (Contains<T>(name))
                throw new ArgumentException("The specified type or name of object pool is exist.");

            var type = typeof(T);
            var pool = new ObjectPool<T>(poolSize, createObjectFunc, getObjectHandler);

            UpdateObjectPools(type, name, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="policy"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObjectPayloadPool<T> Create<T>(IPolicy<T> policy)
        {
            if (policy is null)
                throw new ArgumentNullException(nameof(policy));

            if (Contains<T>())
                throw new ArgumentException("The specified type of object pool is exist.");

            var type = typeof(T);
            var pool = new ObjectPool<T>(policy);

            UpdateObjectPools(type, DefaultName, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="pool"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool<T> Create<T>(IObjectPayloadPool<T> pool)
        {
            if (pool is null)
                throw new ArgumentNullException(nameof(pool));

            if (Contains<T>())
                throw new ArgumentException("The specified type of object pool is exist.");

            var type = typeof(T);

            UpdateObjectPools(type, DefaultName, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="poolFunc"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool<T> Create<T>(Func<IObjectPayloadPool<T>> poolFunc)
        {
            if (poolFunc is null)
                throw new ArgumentNullException(nameof(poolFunc));

            if (Contains<T>())
                throw new ArgumentException("The specified type of object pool is exist.");

            var type = typeof(T);
            var pool = poolFunc();

            UpdateObjectPools(type, DefaultName, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="policy"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool<T> Create<T>(string name, IPolicy<T> policy)
        {
            if (policy is null)
                throw new ArgumentNullException(nameof(policy));

            if (string.IsNullOrWhiteSpace(name))
                name = DefaultName;

            if (Contains<T>(name))
                throw new ArgumentException("The specified type or name of object pool is exist.");

            var type = typeof(T);
            var pool = new ObjectPool<T>(policy);

            UpdateObjectPools(type, name, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pool"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool<T> Create<T>(string name, IObjectPayloadPool<T> pool)
        {
            if (pool is null)
                throw new ArgumentNullException(nameof(pool));

            if (Contains<T>(name))
                throw new ArgumentException("The specified type or name of object pool is exist.");

            var type = typeof(T);

            UpdateObjectPools(type, name, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="poolFunc"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool<T> Create<T>(string name, Func<IObjectPayloadPool<T>> poolFunc)
        {
            if (poolFunc is null)
                throw new ArgumentNullException(nameof(poolFunc));

            if (Contains<T>(name))
                throw new ArgumentException("The specified type or name of object pool is exist.");

            var type = typeof(T);
            var pool = poolFunc();

            UpdateObjectPools(type, name, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <returns></returns>
        public static IObjectPayloadPool Create(Type type, int poolSize, Func<object> createObjectFunc, Action<ObjectPayload> getObjectHandler = null)
        {
            if (Contains(type))
                throw new ArgumentException("The specified type of object pool is exist.");

            var pool = new ObjectPool(type, poolSize, createObjectFunc, getObjectHandler);

            UpdateObjectPools(type, DefaultName, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool Create(Type type, string name, int poolSize, Func<object> createObjectFunc, Action<ObjectPayload> getObjectHandler = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = DefaultName;

            if (Contains(type, name))
                throw new ArgumentException("The specified type or name of object pool is exist.");

            var pool = new ObjectPool(type, poolSize, createObjectFunc, getObjectHandler);

            UpdateObjectPools(type, name, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        public static IObjectPayloadPool Create(IPolicy policy)
        {
            if (policy is null)
                throw new ArgumentNullException(nameof(policy));

            if (Contains(policy.BindingType))
                throw new ArgumentException("The specified type of object pool is exist.");

            var pool = new ObjectPool(policy);

            UpdateObjectPools(policy.BindingType, DefaultName, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool Create(IObjectPayloadPool pool)
        {
            if (pool is null)
                throw new ArgumentNullException(nameof(pool));

            if (Contains(pool.Policy.BindingType))
                throw new ArgumentException("The specified type of object pool is exist.");

            UpdateObjectPools(pool.Policy.BindingType, DefaultName, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="poolFunc"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool Create(Type type, Func<IObjectPayloadPool> poolFunc)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (poolFunc is null)
                throw new ArgumentNullException(nameof(poolFunc));

            if (Contains(type))
                throw new ArgumentException("The specified type of object pool is exist.");

            var pool = poolFunc();

            UpdateObjectPools(type, DefaultName, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool Create(string name, IPolicy policy)
        {
            if (policy is null)
                throw new ArgumentNullException(nameof(policy));

            if (string.IsNullOrWhiteSpace(name))
                name = DefaultName;

            if (Contains(policy.BindingType, name))
                throw new ArgumentException("The specified type or name of object pool is exist.");

            var pool = new ObjectPool(policy);

            UpdateObjectPools(policy.BindingType, name, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool Create(string name, IObjectPayloadPool pool)
        {
            if (pool is null)
                throw new ArgumentNullException(nameof(pool));

            if (Contains(pool.Policy.BindingType, name))
                throw new ArgumentException("The specified type or name of object pool is exist.");

            UpdateObjectPools(pool.Policy.BindingType, name, pool);

            return pool;
        }

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="poolFunc"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static IObjectPayloadPool Create(Type type, string name, Func<IObjectPayloadPool> poolFunc)
        {
            if (poolFunc is null)
                throw new ArgumentNullException(nameof(poolFunc));

            if (Contains(type, name))
                throw new ArgumentException("The specified type or name of object pool is exist.");

            var pool = poolFunc();

            UpdateObjectPools(type, name, pool);

            return pool;
        }

        #endregion

        #region Get or create

        /// <summary>
        /// To get or create a specified type of object pool.
        /// </summary>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObjectPayloadPool<T> GetOrCreate<T>(int poolSize, Func<T> createObjectFunc, Action<ObjectPayload<T>> getObjectHandler = null)
        {
            return Contains<T>() ? Get<T>() : Create(poolSize, createObjectFunc, getObjectHandler);
        }

        /// <summary>
        /// To get or create a specified type of object pool.
        /// </summary>
        /// <param name="policy"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObjectPayloadPool<T> GetOrCreate<T>(IPolicy<T> policy)
        {
            return Contains<T>() ? Get<T>() : Create(policy);
        }

        /// <summary>
        /// To get or create a specified type of object pool.
        /// </summary>
        /// <param name="insteadOfFactory"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IObjectPayloadPool<T> GetOrCreate<T>(Func<IObjectPayloadPool<T>> insteadOfFactory)
        {
            if (insteadOfFactory is null)
                throw new ArgumentNullException(nameof(insteadOfFactory));

            if (Contains<T>())
                return Get<T>();

            var pool = insteadOfFactory();

            UpdateObjectPools(typeof(T), DefaultName, pool);

            return pool;
        }

        /// <summary>
        /// To get or create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObjectPayloadPool<T> GetOrCreate<T>(string name, int poolSize, Func<T> createObjectFunc, Action<ObjectPayload<T>> getObjectHandler = null)
        {
            return Contains<T>(name) ? Get<T>(name) : Create(name, poolSize, createObjectFunc, getObjectHandler);
        }

        /// <summary>
        /// To get or create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="policy"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObjectPayloadPool<T> GetOrCreate<T>(string name, IPolicy<T> policy)
        {
            return Contains<T>(name) ? Get<T>(name) : Create(name, policy);
        }

        /// <summary>
        /// To get or create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="insteadOfFactory"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IObjectPayloadPool<T> GetOrCreate<T>(string name, Func<IObjectPayloadPool<T>> insteadOfFactory)
        {
            if (insteadOfFactory is null)
                throw new ArgumentNullException(nameof(insteadOfFactory));

            if (Contains<T>(name))
                return Get<T>(name);

            var pool = insteadOfFactory();

            UpdateObjectPools(typeof(T), name, pool);

            return pool;
        }

        /// <summary>
        /// To get or create a specified type of object pool.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <returns></returns>
        public static IObjectPayloadPool GetOrCreate(Type type, int poolSize, Func<object> createObjectFunc, Action<ObjectPayload> getObjectHandler = null)
        {
            return Contains(type) ? Get(type) : Create(type, poolSize, createObjectFunc, getObjectHandler);
        }

        /// <summary>
        /// To get or create a specified type of object pool.
        /// </summary>
        /// <param name="policy"></param>
        /// <returns></returns>
        public static IObjectPayloadPool GetOrCreate(IPolicy policy)
        {
            if (policy is null)
                throw new ArgumentNullException(nameof(policy));
            return Contains(policy.BindingType) ? Get(policy.BindingType) : Create(policy);
        }

        /// <summary>
        /// To get or create a specified type of object pool.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="insteadOfFactory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IObjectPayloadPool GetOrCreate(Type type, Func<IObjectPayloadPool> insteadOfFactory)
        {
            if (insteadOfFactory is null)
                throw new ArgumentNullException(nameof(insteadOfFactory));

            if (Contains(type))
                return Get(type);

            var pool = insteadOfFactory();

            UpdateObjectPools(type, DefaultName, pool);

            return pool;
        }

        /// <summary>
        /// To get or create a specified type and name of object pool.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <returns></returns>
        public static IObjectPayloadPool GetOrCreate(Type type, string name, int poolSize, Func<object> createObjectFunc, Action<ObjectPayload> getObjectHandler = null)
        {
            return Contains(type, name) ? Get(type, name) : Create(type, name, poolSize, createObjectFunc, getObjectHandler);
        }

        /// <summary>
        /// To get or create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        public static IObjectPayloadPool GetOrCreate(string name, IPolicy policy)
        {
            if (policy is null)
                throw new ArgumentNullException(nameof(policy));
            return Contains(policy.BindingType, name) ? Get(policy.BindingType, name) : Create(name, policy);
        }

        /// <summary>
        /// To get or create a specified type and name of object pool.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="insteadOfFactory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IObjectPayloadPool GetOrCreate(Type type, string name, Func<IObjectPayloadPool> insteadOfFactory)
        {
            if (insteadOfFactory is null)
                throw new ArgumentNullException(nameof(insteadOfFactory));

            if (Contains(type, name))
                return Get(type, name);

            var pool = insteadOfFactory();

            UpdateObjectPools(type, name, pool);

            return pool;
        }

        #endregion

        #region Contains

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Contains<T>()
        {
            return _defaultManagedModel.ContainsDefaultTyped(typeof(T));
        }

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Contains<T>(string name)
        {
            return _defaultManagedModel.Contains(typeof(T), name);
        }

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Contains(Type type)
        {
            return _defaultManagedModel.ContainsDefaultTyped(type);
        }

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Contains(string name)
        {
            return _defaultManagedModel.ContainsNamedSet(name);
        }

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Contains(Type type, string name)
        {
            return _defaultManagedModel.Contains(type, name);
        }

        #endregion

        #region Internal helpers

        private static void UpdateObjectPools(Type type, string name, IDisposable pool)
        {
            _defaultManagedModel.AddOrUpdate(type, name, pool);
        }

        #endregion
    }
}