using System;
using System.Collections.Concurrent;
using Cosmos.Disposables.ObjectPools.Managed;

// ReSharper disable InconsistentNaming

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Object pool manager
    /// </summary>
    public static partial class ObjectPoolManager
    {
        /// <summary>
        /// Managed models
        /// </summary>
        public static class ManagedModels
        {
            /// <summary>
            /// Gets default object pool managed model
            /// </summary>
            public static IObjectPoolManagedModel Default => _defaultManagedModel;

            private static readonly ConcurrentDictionary<Type, IObjectPoolManagedModel> _managedModels;

            static ManagedModels()
            {
                _managedModels = new ConcurrentDictionary<Type, IObjectPoolManagedModel>();
            }

            #region Register

            /// <summary>
            /// Register IObjectPoolManagedModel type
            /// </summary>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public static bool Register<TManagedModel>() where TManagedModel : class, IObjectPoolManagedModel, new()
            {
                if (DefaultManagedModelType<TManagedModel>())
                    throw new ArgumentException($"The default type '{nameof(ObjectPoolManagedModel)}' does not provide registration.");

                if (_managedModels.ContainsKey(typeof(TManagedModel)))
                    throw new ArgumentException("The type has been registered.");

                return _managedModels.TryAdd(typeof(TManagedModel), new TManagedModel());
            }

            /// <summary>
            /// Register IObjectPoolManagedModel type
            /// </summary>
            /// <param name="factory"></param>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static bool Register<TManagedModel>(Func<TManagedModel> factory) where TManagedModel : class, IObjectPoolManagedModel
            {
                if (factory is null)
                    throw new ArgumentNullException(nameof(factory));

                if (DefaultManagedModelType<TManagedModel>())
                    throw new ArgumentException($"The default type '{nameof(ObjectPoolManagedModel)}' does not provide registration.");

                if (_managedModels.ContainsKey(typeof(TManagedModel)))
                    throw new ArgumentException("The type has been registered.");

                var instance = factory();

                if (instance is null)
                    throw new ArgumentException("Cannot create a valid instance through the factory method of IObjectPoolManagedModel.");

                return _managedModels.TryAdd(typeof(TManagedModel), instance);
            }

            #endregion

            #region Get

            /// <summary>
            /// To get the specified type of object pool.<br />
            /// 获取指定类型的对象池。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"> Unknown type.</exception>
            /// <exception cref="ArgumentException">Unable to get the specified type of object pool.</exception>
            public static IObjectPool<T> Get<T, TManagedModel>() where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();
                return model.GetDefaultTyped<T>();
            }

            /// <summary>
            /// To get the specified type and name of object pool.<br />
            /// 获取指定类型和名称的对象池。
            /// </summary>
            /// <param name="name"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"> Unknown type or name.</exception>
            /// <exception cref="ArgumentException">Unable to get the specified type and name of object pool.</exception>
            public static IObjectPool<T> Get<T, TManagedModel>(string name) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();
                return model.Get<T>(name);
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
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            public static IObjectPool<T> Create<T, TManagedModel>(int poolSize, Func<T> createObjectFunc, Action<Object<T>> getObjectHandler = null)
                where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();

                if (model.ContainsDefaultTyped(typeof(T)))
                    throw new ArgumentException("The specified type of object pool is exist.");

                var type = typeof(T);
                var pool = new ObjectPool<T>(poolSize, createObjectFunc, getObjectHandler);

                UpdateObjectPools(model, type, DefaultName, pool);

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
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public static IObjectPool<T> Create<T, TManagedModel>(string name, int poolSize, Func<T> createObjectFunc, Action<Object<T>> getObjectHandler = null)
                where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();

                if (string.IsNullOrWhiteSpace(name))
                    name = DefaultName;

                if (model.Contains(typeof(T), name))
                    throw new ArgumentException("The specified type or name of object pool is exist.");

                var type = typeof(T);
                var pool = new ObjectPool<T>(poolSize, createObjectFunc, getObjectHandler);

                UpdateObjectPools(model, type, name, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type of object pool.
            /// </summary>
            /// <param name="policy"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            public static IObjectPool<T> Create<T, TManagedModel>(IPolicy<T> policy) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();

                if (model.ContainsDefaultTyped(typeof(T)))
                    throw new ArgumentException("The specified type of object pool is exist.");

                var type = typeof(T);
                var pool = new ObjectPool<T>(policy);

                UpdateObjectPools(model, type, DefaultName, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type of object pool.
            /// </summary>
            /// <param name="pool"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static IObjectPool<T> Create<T, TManagedModel>(IObjectPool<T> pool) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();

                if (pool is null)
                    throw new ArgumentNullException(nameof(pool));

                if (model.ContainsDefaultTyped(typeof(T)))
                    throw new ArgumentException("The specified type of object pool is exist.");

                var type = typeof(T);

                UpdateObjectPools(model, type, DefaultName, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type of object pool.
            /// </summary>
            /// <param name="poolFunc"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static IObjectPool<T> Create<T, TManagedModel>(Func<IObjectPool<T>> poolFunc) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();

                if (poolFunc is null)
                    throw new ArgumentNullException(nameof(poolFunc));

                if (model.ContainsDefaultTyped(typeof(T)))
                    throw new ArgumentException("The specified type of object pool is exist.");

                var type = typeof(T);
                var pool = poolFunc();

                UpdateObjectPools(model, type, DefaultName, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type and name of object pool.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="policy"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public static IObjectPool<T> Create<T, TManagedModel>(string name, IPolicy<T> policy) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();

                if (string.IsNullOrWhiteSpace(name))
                    name = DefaultName;

                if (model.Contains(typeof(T), name))
                    throw new ArgumentException("The specified type or name of object pool is exist.");

                var type = typeof(T);
                var pool = new ObjectPool<T>(policy);

                UpdateObjectPools(model, type, name, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type and name of object pool.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="pool"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static IObjectPool<T> Create<T, TManagedModel>(string name, IObjectPool<T> pool) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();

                if (pool is null)
                    throw new ArgumentNullException(nameof(pool));

                if (model.Contains(typeof(T), name))
                    throw new ArgumentException("The specified type or name of object pool is exist.");

                var type = typeof(T);

                UpdateObjectPools(model, type, name, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type and name of object pool.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="poolFunc"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static IObjectPool<T> Create<T, TManagedModel>(string name, Func<IObjectPool<T>> poolFunc) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();

                if (poolFunc is null)
                    throw new ArgumentNullException(nameof(poolFunc));

                if (model.Contains(typeof(T), name))
                    throw new ArgumentException("The specified type or name of object pool is exist.");

                var type = typeof(T);
                var pool = poolFunc();

                UpdateObjectPools(model, type, name, pool);

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
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            public static IObjectPool<T> GetOrCreate<T, TManagedModel>(int poolSize, Func<T> createObjectFunc, Action<Object<T>> getObjectHandler = null)
                where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();
                return model.ContainsDefaultTyped(typeof(T))
                    ? model.GetDefaultTyped<T>()
                    : Create<T, TManagedModel>(poolSize, createObjectFunc, getObjectHandler);
            }

            /// <summary>
            /// To get or create a specified type of object pool.
            /// </summary>
            /// <param name="policy"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            public static IObjectPool<T> GetOrCreate<T, TManagedModel>(IPolicy<T> policy) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();
                return model.ContainsDefaultTyped(typeof(T))
                    ? model.GetDefaultTyped<T>()
                    : Create<T, TManagedModel>(policy);
            }

            /// <summary>
            /// To get or create a specified type of object pool.
            /// </summary>
            /// <param name="insteadOfFactory"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            public static IObjectPool<T> GetOrCreate<T, TManagedModel>(Func<IObjectPool<T>> insteadOfFactory) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();

                if (insteadOfFactory is null)
                    throw new ArgumentNullException(nameof(insteadOfFactory));

                if (model.ContainsDefaultTyped(typeof(T)))
                    return model.GetDefaultTyped<T>();

                var pool = insteadOfFactory();

                UpdateObjectPools(model, typeof(T), DefaultName, pool);

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
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            public static IObjectPool<T> GetOrCreate<T, TManagedModel>(string name, int poolSize, Func<T> createObjectFunc, Action<Object<T>> getObjectHandler = null)
                where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();
                return model.Contains(typeof(T), name)
                    ? model.Get<T>(name)
                    : Create<T, TManagedModel>(name, poolSize, createObjectFunc, getObjectHandler);
            }

            /// <summary>
            /// To get or create a specified type and name of object pool.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="policy"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            public static IObjectPool<T> GetOrCreate<T, TManagedModel>(string name, IPolicy<T> policy) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();
                return model.Contains(typeof(T), name)
                    ? model.Get<T>(name)
                    : Create<T, TManagedModel>(name, policy);
            }

            /// <summary>
            /// To get or create a specified type and name of object pool.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="insteadOfFactory"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            public static IObjectPool<T> GetOrCreate<T, TManagedModel>(string name, Func<IObjectPool<T>> insteadOfFactory) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();

                if (insteadOfFactory is null)
                    throw new ArgumentNullException(nameof(insteadOfFactory));

                if (model.Contains(typeof(T), name))
                    return model.Get<T>(name);

                var pool = insteadOfFactory();

                UpdateObjectPools(model, typeof(T), name, pool);

                return pool;
            }

            #endregion

            #region Contains

            /// <summary>
            /// Contains<br />
            /// 查看是否包含
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            public static bool Contains<T, TManagedModel>() where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();
                return model.ContainsDefaultTyped(typeof(T));
            }

            /// <summary>
            /// Contains<br />
            /// 查看是否包含
            /// </summary>
            /// <param name="name"></param>
            /// <typeparam name="T"></typeparam>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            public static bool Contains<T, TManagedModel>(string name) where TManagedModel : class, IObjectPoolManagedModel
            {
                var model = GetModel<TManagedModel>();
                return model.Contains(typeof(T), name);
            }

            /// <summary>
            /// Contains<br />
            /// 查看是否包含
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="type"></param>
            /// <returns></returns>
            public static bool Contains(Type managedModelType, Type type)
            {
                var model = GetModel(managedModelType);
                return model.ContainsDefaultTyped(type);
            }

            /// <summary>
            /// Contains<br />
            /// 查看是否包含
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="name"></param>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static bool Contains(Type managedModelType, string name)
            {
                var model = GetModel(managedModelType);
                return model.ContainsNamedSet(name);
            }

            /// <summary>
            /// Contains<br />
            /// 查看是否包含
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="type"></param>
            /// <param name="name"></param>
            /// <returns></returns>
            public static bool Contains(Type managedModelType, Type type, string name)
            {
                var model = GetModel(managedModelType);
                return model.Contains(type, name);
            }

            #endregion

            #region Internal helpers

            private static bool DefaultManagedModelType<TManagedModel>() where TManagedModel : class, IObjectPoolManagedModel
                => typeof(TManagedModel) == typeof(ObjectPoolManagedModel);

            private static bool DefaultManagedModelType(Type managedModelType)
                => managedModelType == typeof(ObjectPoolManagedModel);

            private static IObjectPoolManagedModel GetModel<TManagedModel>() where TManagedModel : class, IObjectPoolManagedModel
            {
                IObjectPoolManagedModel model;
                if (DefaultManagedModelType<TManagedModel>())
                    model = _defaultManagedModel;
                else if (_managedModels.TryGetValue(typeof(TManagedModel), out var m))
                    model = m;
                else
                    throw new ArgumentException("Failed to match an instance of the specified IObjectPoolManagedModel type.");
                return model;
            }

            private static IObjectPoolManagedModel GetModel(Type managedModelType)
            {
                IObjectPoolManagedModel model;
                if (DefaultManagedModelType(managedModelType))
                    model = _defaultManagedModel;
                else if (_managedModels.TryGetValue(managedModelType, out var m))
                    model = m;
                else
                    throw new ArgumentException("Failed to match an instance of the specified IObjectPoolManagedModel type.");
                return model;
            }

            private static void UpdateObjectPools(IObjectPoolManagedModel model, Type type, string name, IDisposable pool)
            {
                model.AddOrUpdate(type, name, pool);
            }

            #endregion
        }
    }
}