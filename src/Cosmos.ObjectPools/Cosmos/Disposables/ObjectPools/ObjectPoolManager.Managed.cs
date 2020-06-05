using System;
using System.Collections.Concurrent;
using System.Linq;
using Cosmos.Disposables.ObjectPools.Managed;
using Cosmos.Reflection;

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

            /// <summary>
            /// Register IObjectPoolManagedModel type
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public static bool Register(Type managedModelType)
            {
                if (managedModelType is null)
                    throw new ArgumentNullException(nameof(managedModelType));

                if (DefaultManagedModelType(managedModelType))
                    throw new ArgumentException($"The default type '{nameof(ObjectPoolManagedModel)}' does not provide registration.");

                if (_managedModels.ContainsKey(managedModelType))
                    throw new ArgumentException("The type has been registered.");

                if (!IsValidManagedModel(managedModelType))
                    throw new ArgumentException("This type is not a valid IObjectPoolManagedModel instance.");

                var instance = Types.CreateInstance(managedModelType) as IObjectPoolManagedModel;

                return _managedModels.TryAdd(managedModelType, instance);
            }

            /// <summary>
            /// Register IObjectPoolManagedModel type
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="factory"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static bool Register(Type managedModelType, Func<IObjectPoolManagedModel> factory)
            {
                if (managedModelType is null)
                    throw new ArgumentNullException(nameof(managedModelType));

                if (factory is null)
                    throw new ArgumentNullException(nameof(factory));

                if (DefaultManagedModelType(managedModelType))
                    throw new ArgumentException($"The default type '{nameof(ObjectPoolManagedModel)}' does not provide registration.");

                if (_managedModels.ContainsKey(managedModelType))
                    throw new ArgumentException("The type has been registered.");

                if (!IsValidManagedModel(managedModelType))
                    throw new ArgumentException("This type is not a valid IObjectPoolManagedModel instance.");

                var instance = factory();

                if (instance is null)
                    throw new ArgumentException("Cannot create a valid instance through the factory method of IObjectPoolManagedModel.");

                return _managedModels.TryAdd(managedModelType, instance);
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

            /// <summary>
            /// To get the specified type of object pool.<br />
            /// 获取指定类型的对象池。
            /// </summary>
            /// <param name="type"></param>
            /// <param name="managedModelType"></param>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"> Unknown type.</exception>
            /// <exception cref="ArgumentException">Unable to get the specified type of object pool.</exception>
            public static IObjectPool Get(Type managedModelType, Type type)
            {
                var model = GetModel(managedModelType);
                return model.GetDefaultTyped(type);
            }

            /// <summary>
            /// To get the specified type and name of object pool.<br />
            /// 获取指定类型和名称的对象池。
            /// </summary>
            /// <param name="type"></param>
            /// <param name="name"></param>
            /// <param name="managedModelType"></param>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"> Unknown type or name.</exception>
            /// <exception cref="ArgumentException">Unable to get the specified type and name of object pool.</exception>
            public static IObjectPool Get(Type managedModelType, Type type, string name)
            {
                var model = GetModel(managedModelType);
                return model.Get(type, name);
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
                if (policy is null)
                    throw new ArgumentNullException(nameof(policy));

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

                var type = typeof(T);

                if (model.ContainsDefaultTyped(type))
                    throw new ArgumentException("The specified type of object pool is exist.");

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

                var type = typeof(T);

                if (model.ContainsDefaultTyped(typeof(T)))
                    throw new ArgumentException("The specified type of object pool is exist.");

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

            /// <summary>
            /// Create a specified type of object pool.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="poolSize"></param>
            /// <param name="createObjectFunc"></param>
            /// <param name="getObjectHandler"></param>
            /// <param name="managedModelType"></param>
            /// <returns></returns>
            public static IObjectPool Create(Type managedModelType, Type type, int poolSize, Func<object> createObjectFunc, Action<ObjectOut> getObjectHandler = null)
            {
                var model = GetModel(managedModelType);

                if (model.ContainsDefaultTyped(type))
                    throw new ArgumentException("The specified type of object pool is exist.");

                var pool = new ObjectPool(type, poolSize, createObjectFunc, getObjectHandler);

                UpdateObjectPools(model, type, DefaultName, pool);

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
            /// <param name="managedModelType"></param>
            /// <returns></returns>
            public static IObjectPool Create(Type managedModelType, Type type, string name, int poolSize, Func<object> createObjectFunc, Action<ObjectOut> getObjectHandler = null)
            {
                var model = GetModel(managedModelType);

                if (string.IsNullOrWhiteSpace(name))
                    name = DefaultName;

                if (model.Contains(type, name))
                    throw new ArgumentException("The specified type or name of object pool is exist.");

                var pool = new ObjectPool(type, poolSize, createObjectFunc, getObjectHandler);

                UpdateObjectPools(model, type, name, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type of object pool.
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="policy"></param>
            /// <returns></returns>
            public static IObjectPool Create(Type managedModelType, IPolicy policy)
            {
                if (policy is null)
                    throw new ArgumentNullException(nameof(policy));

                var model = GetModel(managedModelType);

                if (model.ContainsDefaultTyped(policy.BindingType))
                    throw new ArgumentException("The specified type of object pool is exist.");

                var type = policy.BindingType;
                var pool = new ObjectPool(policy);

                UpdateObjectPools(model, type, DefaultName, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type of object pool.
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="pool"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static IObjectPool Create(Type managedModelType, IObjectPool pool)
            {
                var model = GetModel(managedModelType);

                if (pool is null)
                    throw new ArgumentNullException(nameof(pool));

                var type = pool.Policy.BindingType;

                if (model.ContainsDefaultTyped(type))
                    throw new ArgumentException("The specified type of object pool is exist.");

                UpdateObjectPools(model, type, DefaultName, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type of object pool.
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="poolFunc"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static IObjectPool Create(Type managedModelType, Func<IObjectPool> poolFunc)
            {
                var model = GetModel(managedModelType);

                if (poolFunc is null)
                    throw new ArgumentNullException(nameof(poolFunc));

                var pool = poolFunc();
                var type = pool.Policy.BindingType;

                if (model.ContainsDefaultTyped(type))
                    throw new ArgumentException("The specified type of object pool is exist.");

                UpdateObjectPools(model, type, DefaultName, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type and name of object pool.
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="name"></param>
            /// <param name="policy"></param>
            /// <returns></returns>
            public static IObjectPool Create(Type managedModelType, string name, IPolicy policy)
            {
                if (policy is null)
                    throw new ArgumentNullException(nameof(policy));

                var model = GetModel(managedModelType);

                if (model.Contains(policy.BindingType, name))
                    throw new ArgumentException("The specified type and name of object pool is exist.");

                var type = policy.BindingType;
                var pool = new ObjectPool(policy);

                UpdateObjectPools(model, type, name, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type and name of object pool.
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="name"></param>
            /// <param name="pool"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static IObjectPool Create(Type managedModelType, string name, IObjectPool pool)
            {
                var model = GetModel(managedModelType);

                if (pool is null)
                    throw new ArgumentNullException(nameof(pool));

                var type = pool.Policy.BindingType;

                if (model.Contains(type, name))
                    throw new ArgumentException("The specified type and name of object pool is exist.");

                UpdateObjectPools(model, type, name, pool);

                return pool;
            }

            /// <summary>
            /// Create a specified type and name of object pool.
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="name"></param>
            /// <param name="poolFunc"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static IObjectPool Create(Type managedModelType, string name, Func<IObjectPool> poolFunc)
            {
                var model = GetModel(managedModelType);

                if (poolFunc is null)
                    throw new ArgumentNullException(nameof(poolFunc));

                var pool = poolFunc();
                var type = pool.Policy.BindingType;

                if (model.Contains(type, name))
                    throw new ArgumentException("The specified type and name of object pool is exist.");

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

            /// <summary>
            /// To get or create a specified type of object pool.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="poolSize"></param>
            /// <param name="createObjectFunc"></param>
            /// <param name="getObjectHandler"></param>
            /// <param name="managedModelType"></param>
            /// <returns></returns>
            public static IObjectPool GetOrCreate(Type managedModelType, Type type, int poolSize, Func<object> createObjectFunc, Action<ObjectOut> getObjectHandler = null)
            {
                var model = GetModel(managedModelType);
                return model.ContainsDefaultTyped(type)
                    ? model.GetDefaultTyped(type)
                    : Create(managedModelType, type, poolSize, createObjectFunc, getObjectHandler);
            }

            /// <summary>
            /// To get or create a specified type of object pool.
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="type"></param>
            /// <param name="policy"></param>
            /// <returns></returns>
            public static IObjectPool GetOrCreate(Type managedModelType, Type type, IPolicy policy)
            {
                var model = GetModel(managedModelType);
                return model.ContainsDefaultTyped(type)
                    ? model.GetDefaultTyped(type)
                    : Create(managedModelType, policy);
            }

            /// <summary>
            /// To get or create a specified type of object pool.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="insteadOfFactory"></param>
            /// <param name="managedModelType"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            public static IObjectPool GetOrCreate(Type managedModelType, Type type, Func<IObjectPool> insteadOfFactory)
            {
                var model = GetModel(managedModelType);

                if (insteadOfFactory is null)
                    throw new ArgumentNullException(nameof(insteadOfFactory));

                if (model.ContainsDefaultTyped(type))
                    return model.GetDefaultTyped(type);

                var pool = insteadOfFactory();

                UpdateObjectPools(model, type, DefaultName, pool);

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
            /// <param name="managedModelType"></param>
            /// <returns></returns>
            public static IObjectPool GetOrCreate(Type managedModelType, Type type, string name, int poolSize, Func<object> createObjectFunc,
                Action<ObjectOut> getObjectHandler = null)
            {
                var model = GetModel(managedModelType);
                return model.Contains(type, name)
                    ? model.Get(type, name)
                    : Create(managedModelType, type, name, poolSize, createObjectFunc, getObjectHandler);
            }

            /// <summary>
            /// To get or create a specified type and name of object pool.
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <param name="type"></param>
            /// <param name="name"></param>
            /// <param name="policy"></param>
            /// <returns></returns>
            public static IObjectPool GetOrCreate(Type managedModelType, Type type, string name, IPolicy policy)
            {
                var model = GetModel(managedModelType);
                return model.Contains(type, name)
                    ? model.Get(type, name)
                    : Create(managedModelType, name, policy);
            }

            /// <summary>
            /// To get or create a specified type and name of object pool.
            /// </summary>
            /// <param name="type"></param>
            /// <param name="name"></param>
            /// <param name="insteadOfFactory"></param>
            /// <param name="managedModelType"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            public static IObjectPool GetOrCreate(Type managedModelType, Type type, string name, Func<IObjectPool> insteadOfFactory)
            {
                var model = GetModel(managedModelType);

                if (insteadOfFactory is null)
                    throw new ArgumentNullException(nameof(insteadOfFactory));

                if (model.Contains(type, name))
                    return model.Get(type, name);

                var pool = insteadOfFactory();

                UpdateObjectPools(model, type, name, pool);

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

            private static bool IsValidManagedModel(Type managedModelType)
            {
                return managedModelType != null
                    && managedModelType.GetInterfaces().Any(i => i == typeof(IObjectPoolManagedModel));
            }

            #endregion
        }
    }
}