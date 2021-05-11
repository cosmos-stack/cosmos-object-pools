using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Cosmos.Disposables.ObjectPools.Managed
{
    /// <summary>
    /// Object pool managed model
    /// </summary>
    public class ObjectPoolManagedModel : IObjectPoolManagedModel
    {
        private readonly ConcurrentDictionary<Type, IDisposable> _defaultTypedObjectPools;
        private readonly ConcurrentDictionary<(Type, string), IDisposable> _namedTypedObjectPools;
        private readonly ConcurrentDictionary<string, List<IDisposable>> _namedObjectPools;
        private readonly object _updateLockObj = new();

        /// <summary>
        /// Create a new instance of <see cref="ObjectPoolManagedModel"/>.
        /// </summary>
        public ObjectPoolManagedModel()
        {
            _defaultTypedObjectPools = new ConcurrentDictionary<Type, IDisposable>();
            _namedTypedObjectPools = new ConcurrentDictionary<(Type, string), IDisposable>();
            _namedObjectPools = new ConcurrentDictionary<string, List<IDisposable>>();
        }

        /// <inheritdoc />
        public IObjectPool<T> GetDefaultTyped<T>()
        {
            if (_defaultTypedObjectPools.TryGetValue(typeof(T), out var mid))
                if (mid is IObjectPool<T> pool)
                    return pool;
                else if (mid is IObjectPool)
                    throw new ArgumentException("Use the non-generic version of 'GetDefaultTyped' method.");
                else
                    throw new InvalidOperationException($"Unknown type: {typeof(T)}");
            throw new ArgumentException("Unable to get the specified type of object pool.");
        }

        /// <inheritdoc />
        public IObjectPool GetDefaultTyped(Type type)
        {
            if (_defaultTypedObjectPools.TryGetValue(type, out var mid))
                if (mid is IObjectPool pool)
                    return pool;
                else
                    throw new InvalidOperationException($"Unknown type: {type}");
            throw new ArgumentException("Unable to get the specified type of object pool.");
        }

        /// <inheritdoc />
        public IObjectPool<T> Get<T>(string name)
        {
            if (_namedTypedObjectPools.TryGetValue((typeof(T), name), out var mid))
                if (mid is IObjectPool<T> pool)
                    return pool;
                else if (mid is IObjectPool)
                    throw new ArgumentException("Use the non-generic version of 'GetDefaultTyped' method.");
                else
                    throw new InvalidOperationException($"Unknown type ('{typeof(T)}') or name ('{name}').");
            throw new ArgumentException("Unable to get the specified type and name of object pool.");
        }

        /// <inheritdoc />
        public IObjectPool Get(Type type, string name)
        {
            if (_namedTypedObjectPools.TryGetValue((type, name), out var mid))
                if (mid is IObjectPool pool)
                    return pool;
                else
                    throw new InvalidOperationException($"Unknown type ('{type}') or name ('{name}').");
            throw new ArgumentException("Unable to get the specified type and name of object pool.");
        }

        /// <inheritdoc />
        public void AddOrUpdate(Type type, string name, IDisposable pool)
        {
            if (pool is null)
                throw new ArgumentNullException(nameof(pool));

            if (type is null)
                throw new ArgumentNullException(nameof(type));

            lock (_updateLockObj)
            {
                if (DefaultTypedState(name))
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

        /// <inheritdoc />
        public IDisposable Remove(Type type, string name)
        {
            lock (_updateLockObj)
            {
                IDisposable ret = null;

                if (DefaultTypedState(name))
                    _defaultTypedObjectPools.TryRemove(type, out _);

                if (ContainsTypeAndName(type, name))
                    _namedTypedObjectPools.TryRemove((type, name), out ret);

                if (ContainsName(name))
                {
                    var namedPool = _namedObjectPools[name];
                    namedPool.Remove(ret);

                    if (namedPool.Count == 0)
                        _namedObjectPools.TryRemove(name, out _);
                }

                return ret;
            }
        }

        /// <inheritdoc />
        public bool Contains(Type type, string name)
        {
            lock (_updateLockObj)
            {
                return _namedTypedObjectPools.ContainsKey((type, name));
            }
        }

        /// <inheritdoc />
        public bool ContainsDefaultTyped(Type type)
        {
            lock (_updateLockObj)
            {
                return _defaultTypedObjectPools.ContainsKey(type) && _namedTypedObjectPools.ContainsKey((type, ObjectPoolManager.DefaultName));
            }
        }

        /// <inheritdoc />
        public bool ContainsNamedSet(string name)
        {
            lock (_updateLockObj)
            {
                return _namedObjectPools.ContainsKey(name);
            }
        }

        private bool DefaultTypedState(string name)
        {
            return string.Compare(name, ObjectPoolManager.DefaultName, StringComparison.Ordinal) == 0;
        }

        private bool ContainsName(string name)
        {
            return _namedObjectPools.ContainsKey(name);
        }

        private bool ContainsTypeAndName(Type type, string name)
        {
            return _namedTypedObjectPools.ContainsKey((type, name));
        }
    }
}