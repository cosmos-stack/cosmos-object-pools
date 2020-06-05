using System;
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
        /// Managed models for <typeparamref name="TManagedModel"/>
        /// </summary>
        /// <typeparam name="TManagedModel"></typeparam>
        public static class Managed<TManagedModel> where TManagedModel : class, IObjectPoolManagedModel, new()
        {
            #region Register

            /// <summary>
            /// Register IObjectPoolManagedModel type
            /// </summary>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            public static bool Register() => ManagedModels.Register<TManagedModel>();

            /// <summary>
            /// Register IObjectPoolManagedModel type
            /// </summary>
            /// <param name="factory"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public static bool Register(Func<TManagedModel> factory) => ManagedModels.Register(factory);

            #endregion

            #region Get

            /// <summary>
            /// To get the specified type of object pool.<br />
            /// 获取指定类型的对象池。
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"> Unknown type.</exception>
            /// <exception cref="ArgumentException">Unable to get the specified type of object pool.</exception>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> Get<T>() => ManagedModels.Get<T, TManagedModel>();

            /// <summary>
            /// To get the specified type and name of object pool.<br />
            /// 获取指定类型和名称的对象池。
            /// </summary>
            /// <param name="name"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"> Unknown type or name.</exception>
            /// <exception cref="ArgumentException">Unable to get the specified type and name of object pool.</exception>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> Get<T>(string name) => ManagedModels.Get<T, TManagedModel>(name);

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
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> Create<T>(int poolSize, Func<T> createObjectFunc, Action<ObjectOut<T>> getObjectHandler = null)
                => ManagedModels.Create<T, TManagedModel>(poolSize, createObjectFunc, getObjectHandler);

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
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> Create<T>(string name, int poolSize, Func<T> createObjectFunc, Action<ObjectOut<T>> getObjectHandler = null)
                => ManagedModels.Create<T, TManagedModel>(name, poolSize, createObjectFunc, getObjectHandler);

            /// <summary>
            /// Create a specified type of object pool.
            /// </summary>
            /// <param name="policy"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> Create<T>(IPolicy<T> policy)
                => ManagedModels.Create<T, TManagedModel>(policy);

            /// <summary>
            /// Create a specified type of object pool.
            /// </summary>
            /// <param name="pool"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> Create<T>(IObjectPool<T> pool)
                => ManagedModels.Create<T, TManagedModel>(pool);

            /// <summary>
            /// Create a specified type of object pool.
            /// </summary>
            /// <param name="poolFunc"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> Create<T>(Func<IObjectPool<T>> poolFunc)
                => ManagedModels.Create<T, TManagedModel>(poolFunc);

            /// <summary>
            /// Create a specified type and name of object pool.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="policy"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentException"></exception>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> Create<T>(string name, IPolicy<T> policy)
                => ManagedModels.Create<T, TManagedModel>(name, policy);

            /// <summary>
            /// Create a specified type and name of object pool.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="pool"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> Create<T>(string name, IObjectPool<T> pool)
                => ManagedModels.Create<T, TManagedModel>(name, pool);

            /// <summary>
            /// Create a specified type and name of object pool.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="poolFunc"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> Create<T>(string name, Func<IObjectPool<T>> poolFunc)
                => ManagedModels.Create<T, TManagedModel>(name, poolFunc);

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
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> GetOrCreate<T>(int poolSize, Func<T> createObjectFunc, Action<ObjectOut<T>> getObjectHandler = null)
                => ManagedModels.GetOrCreate<T, TManagedModel>(poolSize, createObjectFunc, getObjectHandler);

            /// <summary>
            /// To get or create a specified type of object pool.
            /// </summary>
            /// <param name="policy"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> GetOrCreate<T>(IPolicy<T> policy)
                => ManagedModels.GetOrCreate<T, TManagedModel>(policy);

            /// <summary>
            /// To get or create a specified type of object pool.
            /// </summary>
            /// <param name="insteadOfFactory"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> GetOrCreate<T>(Func<IObjectPool<T>> insteadOfFactory)
                => ManagedModels.GetOrCreate<T, TManagedModel>(insteadOfFactory);

            /// <summary>
            /// To get or create a specified type and name of object pool.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="poolSize"></param>
            /// <param name="createObjectFunc"></param>
            /// <param name="getObjectHandler"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> GetOrCreate<T>(string name, int poolSize, Func<T> createObjectFunc, Action<ObjectOut<T>> getObjectHandler = null)
                => ManagedModels.GetOrCreate<T, TManagedModel>(name, poolSize, createObjectFunc, getObjectHandler);

            /// <summary>
            /// To get or create a specified type and name of object pool.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="policy"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> GetOrCreate<T>(string name, IPolicy<T> policy)
                => ManagedModels.GetOrCreate<T, TManagedModel>(name, policy);

            /// <summary>
            /// To get or create a specified type and name of object pool.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="insteadOfFactory"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static IObjectPool<T> GetOrCreate<T>(string name, Func<IObjectPool<T>> insteadOfFactory)
                => ManagedModels.GetOrCreate<T, TManagedModel>(name, insteadOfFactory);

            #endregion

            #region Contains

            /// <summary>
            /// Contains<br />
            /// 查看是否包含
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static bool Contains<T>() => ManagedModels.Contains<T, TManagedModel>();

            /// <summary>
            /// Contains<br />
            /// 查看是否包含
            /// </summary>
            /// <param name="name"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static bool Contains<T>(string name) => ManagedModels.Contains<T, TManagedModel>(name);

            /// <summary>
            /// Contains<br />
            /// 查看是否包含
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static bool Contains(Type type) => ManagedModels.Contains(typeof(TManagedModel), type);

            /// <summary>
            /// Contains<br />
            /// 查看是否包含
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static bool Contains(string name) => ManagedModels.Contains(typeof(TManagedModel), name);

            /// <summary>
            /// Contains<br />
            /// 查看是否包含
            /// </summary>
            /// <param name="type"></param>
            /// <param name="name"></param>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static bool Contains(Type type, string name) => ManagedModels.Contains(typeof(TManagedModel), type, name);

            #endregion
        }
    }
}