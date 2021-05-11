using System;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Extensions for policy
    /// </summary>
    public static class PolicyExtensions
    {
        /// <summary>
        /// Register this policy for object pool
        /// </summary>
        /// <param name="policy"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Register<T>(this IPolicy<T> policy)
        {
            if (policy is null)
                throw new ArgumentNullException(nameof(policy));
            ObjectPoolManager.Create(policy);
        }

        /// <summary>
        /// Register this policy for object pool safety
        /// </summary>
        /// <param name="policy"></param>
        /// <typeparam name="T"></typeparam>
        public static void SafeRegister<T>(this IPolicy<T> policy)
        {
            if (policy is null)
                return;
            if (ObjectPoolManager.Contains<T>())
                return;
            ObjectPoolManager.Create(policy);
        }

        /// <summary>
        /// Try register this policy for object pool.
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="pool"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryRegister<T>(this IPolicy<T> policy, out IObjectPool<T> pool)
        {
            try
            {
                if (policy is null)
                    throw new ArgumentNullException(nameof(policy));
                pool = ObjectPoolManager.Create(policy);
                return true;
            }
            catch
            {
                pool = default;
                return false;
            }
        }

        /// <summary>
        /// Register this policy for object pool
        /// </summary>
        /// <param name="policy"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void Register(this IPolicy policy)
        {
            if (policy is null)
                throw new ArgumentNullException(nameof(policy));
            ObjectPoolManager.Create(policy);
        }

        /// <summary>
        /// Register this policy for object pool safety
        /// </summary>
        /// <param name="policy"></param>
        public static void SafeRegister(this IPolicy policy)
        {
            if (policy is null)
                return;
            if (ObjectPoolManager.Contains(policy.BindingType))
                return;
            ObjectPoolManager.Create(policy);
        }

        /// <summary>
        /// Try register this policy for object pool.
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryRegister(this IPolicy policy, out IObjectPool pool)
        {
            try
            {
                if (policy is null)
                    throw new ArgumentNullException(nameof(policy));
                pool = ObjectPoolManager.Create(policy);
                return true;
            }
            catch
            {
                pool = default;
                return false;
            }
        }
    }
}