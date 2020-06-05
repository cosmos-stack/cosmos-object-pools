using System;
using Cosmos.Disposables.ObjectPools.Managed;
using Opt = Cosmos.Disposables.ObjectPools.ObjectPoolManager.NonGeneric.ManagedModelOpt;

// ReSharper disable InconsistentNaming

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Object pool manager
    /// </summary>
    public static partial class ObjectPoolManager
    {
        /// <summary>
        /// Non-Generic
        /// </summary>
        public static class NonGeneric
        {
            /// <summary>
            /// Managed
            /// </summary>
            /// <param name="managedModelType"></param>
            /// <returns></returns>
            public static Opt Managed(Type managedModelType) => new Opt(managedModelType);

            /// <summary>
            /// Managed
            /// </summary>
            /// <typeparam name="TManagedModel"></typeparam>
            /// <returns></returns>
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static Opt Managed<TManagedModel>() where TManagedModel : class, IObjectPoolManagedModel => new Opt(typeof(TManagedModel));

            /// <summary>
            /// Managed model operator
            /// </summary>
            public struct ManagedModelOpt
            {
                internal ManagedModelOpt(Type managedModelType)
                {
                    ManagedModelType = managedModelType ?? throw new ArgumentNullException(nameof(managedModelType));
                }

                /// <summary>
                /// Managed model type
                /// </summary>
                public Type ManagedModelType { get; }
            }
        }
    }

    /// <summary>
    /// Extensions for Non-Generic ObjectPool manager
    /// </summary>
    public static class ObjectPoolManagerNonGenericExtensions
    {
        #region Register

        /// <summary>
        /// Register IObjectPoolManagedModel type
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static bool Register(this Opt opt)
            => ObjectPoolManager.ManagedModels.Register(opt.ManagedModelType);

        /// <summary>
        /// Register IObjectPoolManagedModel type
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static bool Register(this Opt opt, Func<IObjectPoolManagedModel> factory)
            => ObjectPoolManager.ManagedModels.Register(opt.ManagedModelType, factory);

        #endregion

        #region Get

        /// <summary>
        /// To get the specified type of object pool.<br />
        /// 获取指定类型的对象池。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"> Unknown type.</exception>
        /// <exception cref="ArgumentException">Unable to get the specified type of object pool.</exception>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static IObjectPool Get(this Opt opt, Type type)
            => ObjectPoolManager.ManagedModels.Get(opt.ManagedModelType, type);

        /// <summary>
        /// To get the specified type and name of object pool.<br />
        /// 获取指定类型和名称的对象池。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"> Unknown type or name.</exception>
        /// <exception cref="ArgumentException">Unable to get the specified type and name of object pool.</exception>
        public static IObjectPool Get(this Opt opt, Type type, string name)
            => ObjectPoolManager.ManagedModels.Get(opt.ManagedModelType, type, name);

        #endregion

        #region Create

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static IObjectPool Create<T>(this Opt opt, int poolSize, Func<object> createObjectFunc, Action<ObjectOut> getObjectHandler = null)
            => ObjectPoolManager.ManagedModels.Create(opt.ManagedModelType, typeof(T), poolSize, createObjectFunc, getObjectHandler);

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static IObjectPool Create<T>(this Opt opt, string name, int poolSize, Func<object> createObjectFunc, Action<ObjectOut> getObjectHandler = null)
            => ObjectPoolManager.ManagedModels.Create(opt.ManagedModelType, typeof(T), name, poolSize, createObjectFunc, getObjectHandler);

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static IObjectPool Create(this Opt opt, Type type, int poolSize, Func<object> createObjectFunc, Action<ObjectOut> getObjectHandler = null)
            => ObjectPoolManager.ManagedModels.Create(opt.ManagedModelType, type, poolSize, createObjectFunc, getObjectHandler);

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static IObjectPool Create(this Opt opt, Type type, string name, int poolSize, Func<object> createObjectFunc, Action<ObjectOut> getObjectHandler = null)
            => ObjectPoolManager.ManagedModels.Create(opt.ManagedModelType, type, name, poolSize, createObjectFunc, getObjectHandler);

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        public static IObjectPool Create(this Opt opt, IPolicy policy)
            => ObjectPoolManager.ManagedModels.Create(opt.ManagedModelType, policy);

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        public static IObjectPool Create(this Opt opt, IObjectPool pool)
            => ObjectPoolManager.ManagedModels.Create(opt.ManagedModelType, pool);

        /// <summary>
        /// Create a specified type of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="poolFunc"></param>
        /// <returns></returns>
        public static IObjectPool Create(this Opt opt, Func<IObjectPool> poolFunc)
            => ObjectPoolManager.ManagedModels.Create(opt.ManagedModelType, poolFunc);

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="name"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        public static IObjectPool Create(this Opt opt, string name, IPolicy policy)
            => ObjectPoolManager.ManagedModels.Create(opt.ManagedModelType, name, policy);

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="name"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        public static IObjectPool Create(this Opt opt, string name, IObjectPool pool)
            => ObjectPoolManager.ManagedModels.Create(opt.ManagedModelType, name, pool);

        /// <summary>
        /// Create a specified type and name of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="name"></param>
        /// <param name="poolFunc"></param>
        /// <returns></returns>
        public static IObjectPool Create(this Opt opt, string name, Func<IObjectPool> poolFunc)
            => ObjectPoolManager.ManagedModels.Create(opt.ManagedModelType, name, poolFunc);

        #endregion

        #region Get or create

        /// <summary>
        /// To get or create a specified type of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="type"></param>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <returns></returns>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static IObjectPool GetOrCreate(this Opt opt, Type type, int poolSize, Func<object> createObjectFunc, Action<ObjectOut> getObjectHandler = null)
            => ObjectPoolManager.ManagedModels.GetOrCreate(opt.ManagedModelType, type, poolSize, createObjectFunc, getObjectHandler);

        /// <summary>
        /// To get or create a specified type of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="type"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static IObjectPool GetOrCreate(this Opt opt, Type type, IPolicy policy)
            => ObjectPoolManager.ManagedModels.GetOrCreate(opt.ManagedModelType, type, policy);

        /// <summary>
        /// To get or create a specified type of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="insteadOfFactory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static IObjectPool GetOrCreate(this Opt opt, Type type, Func<IObjectPool> insteadOfFactory)
            => ObjectPoolManager.ManagedModels.GetOrCreate(opt.ManagedModelType, type, insteadOfFactory);

        /// <summary>
        /// To get or create a specified type and name of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="poolSize"></param>
        /// <param name="createObjectFunc"></param>
        /// <param name="getObjectHandler"></param>
        /// <returns></returns>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static IObjectPool GetOrCreate(this Opt opt, Type type, string name, int poolSize, Func<object> createObjectFunc, Action<ObjectOut> getObjectHandler = null)
            => ObjectPoolManager.ManagedModels.GetOrCreate(opt.ManagedModelType, type, name, poolSize, createObjectFunc, getObjectHandler);

        /// <summary>
        /// To get or create a specified type and name of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="policy"></param>
        /// <returns></returns>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static IObjectPool GetOrCreate(this Opt opt, Type type, string name, IPolicy policy)
            => ObjectPoolManager.ManagedModels.GetOrCreate(opt.ManagedModelType, type, name, policy);

        /// <summary>
        /// To get or create a specified type and name of object pool.
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="insteadOfFactory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public static IObjectPool GetOrCreate(this Opt opt, Type type, string name, Func<IObjectPool> insteadOfFactory)
            => ObjectPoolManager.ManagedModels.GetOrCreate(opt.ManagedModelType, type, name, insteadOfFactory);

        #endregion

        #region Contains

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Contains<T>(this Opt opt) => ObjectPoolManager.ManagedModels.Contains(opt.ManagedModelType, typeof(T));

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <param name="name"></param>
        /// <param name="opt"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Contains<T>(this Opt opt, string name) => ObjectPoolManager.ManagedModels.Contains(opt.ManagedModelType, typeof(T), name);

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Contains(this Opt opt, Type type) => ObjectPoolManager.ManagedModels.Contains(opt.ManagedModelType, type);

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Contains(this Opt opt, string name) => ObjectPoolManager.ManagedModels.Contains(opt.ManagedModelType, name);

        /// <summary>
        /// Contains<br />
        /// 查看是否包含
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Contains(this Opt opt, Type type, string name) => ObjectPoolManager.ManagedModels.Contains(opt.ManagedModelType, type, name);

        #endregion
    }
}