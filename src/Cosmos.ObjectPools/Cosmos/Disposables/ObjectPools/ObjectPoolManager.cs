using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Cosmos.Disposables.ObjectPools.Abstractions;

// ReSharper disable InconsistentNaming

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Object pool manager
    /// </summary>
    public static class ObjectPoolManager
    {
        private const string DefaultName = "__default";

        private static readonly ConcurrentDictionary<Type, IDisposable> _defaultTypedObjectPools;
        private static readonly ConcurrentDictionary<(Type, string), IDisposable> _namedTypedObjectPools;
        private static readonly ConcurrentDictionary<string, List<IDisposable>> _namedObjectPools;
        private static readonly object _updateLockObj = new object();

        static ObjectPoolManager()
        {
            _defaultTypedObjectPools = new ConcurrentDictionary<Type, IDisposable>();
            _namedTypedObjectPools = new ConcurrentDictionary<(Type, string), IDisposable>();
            _namedObjectPools = new ConcurrentDictionary<string, List<IDisposable>>();
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
        public static IObjectPool<T> Get<T>()
        {
            if (_defaultTypedObjectPools.TryGetValue(typeof(T), out var mid))
                if (mid is IObjectPool<T> pool)
                    return pool;
                else
                    throw new InvalidOperationException($"Unknown type: {typeof(T)}");
            throw new ArgumentException("Unable to get the specified type of object pool.");
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
        public static IObjectPool<T> Get<T>(string name)
        {
            if (_namedTypedObjectPools.TryGetValue((typeof(T), name), out var mid))
                if (mid is IObjectPool<T> pool)
                    return pool;
                else
                    throw new InvalidOperationException($"Unknown type ('{typeof(T)}') or name ('{name}').");
            throw new ArgumentException("Unable to get the specified type and name of object pool.");
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
        public static IObjectPool<T> Create<T>(int poolSize, Func<T> createObjectFunc, Action<Object<T>> getObjectHandler = null)
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
        public static IObjectPool<T> Create<T>(string name, int poolSize, Func<T> createObjectFunc, Action<Object<T>> getObjectHandler = null)
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
        public static IObjectPool<T> Create<T>(IPolicy<T> policy)
        {
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
        public static IObjectPool<T> Create<T>(IObjectPool<T> pool)
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
        public static IObjectPool<T> Create<T>(Func<IObjectPool<T>> poolFunc)
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
        public static IObjectPool<T> Create<T>(string name, IPolicy<T> policy)
        {
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
        public static IObjectPool<T> Create<T>(string name, IObjectPool<T> pool)
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
        public static IObjectPool<T> Create<T>(string name, Func<IObjectPool<T>> poolFunc)
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
        public static IObjectPool<T> GetOrCreate<T>(int poolSize, Func<T> createObjectFunc, Action<Object<T>> getObjectHandler = null)
        {
            if (Contains<T>())
                return Get<T>();
            return Create(poolSize, createObjectFunc, getObjectHandler);
        }

        /// <summary>
        /// To get or create a specified type of object pool.
        /// </summary>
        /// <param name="policy"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObjectPool<T> GetOrCreate<T>(IPolicy<T> policy)
        {
            if (Contains<T>())
                return Get<T>();
            return Create(policy);
        }

        /// <summary>
        /// To get or create a specified type of object pool.
        /// </summary>
        /// <param name="insteadOfFactory"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IObjectPool<T> GetOrCreate<T>(Func<IObjectPool<T>> insteadOfFactory)
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
        public static IObjectPool<T> GetOrCreate<T>(string name, int poolSize, Func<T> createObjectFunc, Action<Object<T>> getObjectHandler = null)
        {
            if (Contains<T>(name))
                return Get<T>(name);
            return Create(name, poolSize, createObjectFunc, getObjectHandler);
        }

        /// <summary>
        /// To get or create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="policy"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObjectPool<T> GetOrCreate<T>(string name, IPolicy<T> policy)
        {
            if (Contains<T>(name))
                return Get<T>(name);
            return Create(name, policy);
        }

        /// <summary>
        /// To get or create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="insteadOfFactory"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IObjectPool<T> GetOrCreate<T>(string name, Func<IObjectPool<T>> insteadOfFactory)
        {
            if (insteadOfFactory is null)
                throw new ArgumentNullException(nameof(insteadOfFactory));

            if (Contains<T>(name))
                return Get<T>(name);

            var pool = insteadOfFactory();

            UpdateObjectPools(typeof(T), name, pool);

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
            return _defaultTypedObjectPools.ContainsKey(typeof(T));
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
            return _namedTypedObjectPools.ContainsKey((typeof(T), name));
        }

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Contains(Type type)
        {
            return _defaultTypedObjectPools.ContainsKey(type);
        }

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Contains(string name)
        {
            return _namedObjectPools.ContainsKey(name);
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
            return _namedTypedObjectPools.ContainsKey((type, name));
        }

        #endregion

        #region Internal helpers

        private static void UpdateObjectPools(Type type, string name, IDisposable pool)
        {
            lock (_updateLockObj)
            {
                _defaultTypedObjectPools.TryAdd(type, pool);
                _namedTypedObjectPools.TryAdd((type, name), pool);
                _namedObjectPools.AddOrUpdate(
                    name,
                    k => new List<IDisposable> {pool},
                    (k, v) =>
                    {
                        v.Add(pool);
                        return v;
                    });
            }
        }

        #endregion
    }
}