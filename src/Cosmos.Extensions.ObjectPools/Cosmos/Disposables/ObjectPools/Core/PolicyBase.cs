using System;
using System.Threading.Tasks;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Policy base
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="T"></typeparam>
    public abstract class PolicyBase<T, TObject> : IPolicyCore<T, TObject>
        where TObject : ObjectBoxBase<T>, IObjectBox
    {
        /// <inheritdoc />
        public string Name { get; set; }

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

        /// <inheritdoc />
        public abstract T OnCreate();

        /// <inheritdoc />
        public void OnDestroy(T obj) { }

        /// <inheritdoc />
        public void OnGetTimeout() { }

        /// <inheritdoc />
        public void OnAvailable() { }

        /// <inheritdoc />
        public void OnUnavailable() { }

        /// <inheritdoc />
        public void OnGet(TObject obj) { }

        /// <inheritdoc />
        public abstract Task OnGetAsync(TObject obj);

        /// <inheritdoc />
        public void OnReturn(TObject obj) { }

        /// <inheritdoc />
        public bool OnCheckAvailable(TObject obj) => true;
    }
}