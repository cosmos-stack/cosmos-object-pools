using System;
using System.Threading.Tasks;
using Cosmos.Asynchronous;
using Cosmos.Disposables.ObjectPools.Abstractions;
using Cosmos.Reflection;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Default policy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultPolicy<T> : IPolicy<T>
    {
        /// <inheritdoc />
        public string Name { get; set; } = Types.Of<DefaultPolicy<T>>().FullName;

        /// <inheritdoc />
        public int PoolSize { get; set; } = 1_000;

        /// <inheritdoc />
        public TimeSpan SyncGetTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <inheritdoc />
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromSeconds(50);

        /// <inheritdoc />
        public int AsyncGetCapacity { get; set; } = 10_000;

        /// <inheritdoc />
        public bool IsThrowGetTimeoutException { get; set; } = true;

        /// <inheritdoc />
        public bool IsAutoDisposeWithSystem { get; set; } = true;

        /// <inheritdoc />
        public int CheckAvailableInterval { get; set; } = 5;

        /// <summary>
        /// Create object
        /// </summary>
        public Func<T> CreateObject;

        /// <summary>
        /// On get object
        /// </summary>
        public Action<Object<T>> OnGetObject;

        /// <inheritdoc />
        public T OnCreate() => CreateObject();

        /// <inheritdoc />
        public void OnDestroy(T obj) { }

        /// <inheritdoc />
        public void OnGet(Object<T> obj) { }

        /// <inheritdoc />
        public Task OnGetAsync(Object<T> obj)
        {
            OnGetObject?.Invoke(obj);
            return Tasks.CompletedTask();
        }

        /// <inheritdoc />
        public void OnGetTimeout() { }

        /// <inheritdoc />
        public void OnReturn(Object<T> obj) { }

        /// <inheritdoc />
        public bool OnCheckAvailable(Object<T> obj) => true;

        /// <inheritdoc />
        public void OnAvailable() { }

        /// <inheritdoc />
        public void OnUnavailable() { }
    }
}