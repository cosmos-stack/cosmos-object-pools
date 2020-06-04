using System;

namespace Cosmos.Disposables.ObjectPools.Managed
{
    /// <summary>
    /// Interface of object pool managed model
    /// </summary>
    public interface IObjectPoolManagedModel
    {
        /// <summary>
        /// To get the specified type of object pool.<br />
        /// 获取指定类型的对象池。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"> Unknown type.</exception>
        /// <exception cref="ArgumentException">Unable to get the specified type of object pool.</exception>
        IObjectPool<T> GetDefaultTyped<T>();

        /// <summary>
        /// To get the specified type and name of object pool.<br />
        /// 获取指定类型和名称的对象池。
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"> Unknown type or name.</exception>
        /// <exception cref="ArgumentException">Unable to get the specified type and name of object pool.</exception>
        IObjectPool<T> Get<T>(string name);

        /// <summary>
        /// Add or update
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="pool"></param>
        void AddOrUpdate(Type type, string name, IDisposable pool);

        /// <summary>
        /// Remove
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        IDisposable Remove(Type type, string name);

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Contains(Type type, string name);

        /// <summary>
        /// Contains default named type object pool
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool ContainsDefaultTyped(Type type);

        /// <summary>
        /// Contains default named type object pool
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool ContainsNamedSet(string name);
    }
}