using System;
using System.Threading.Tasks;

namespace CosmosStack.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Policy base
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="T"></typeparam>
    public abstract class PolicyBase<T, TObject> : IPolicyCore<T, TObject>
        where TObject : ObjectCell<T>, IObjectCell
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
        public virtual void OnDestroy(T obj) { }

        /// <inheritdoc />
        public virtual void OnAcquireTimeout() { }

        /// <inheritdoc />
        public virtual void OnAvailable() { }

        /// <inheritdoc />
        public virtual void OnUnavailable() { }

        /// <inheritdoc />
        public virtual void OnAcquire(TObject obj) { }

        /// <inheritdoc />
        public abstract Task OnAcquireAsync(TObject obj);

        /// <inheritdoc />
        public virtual void OnRecycle(TObject obj) { }

        /// <inheritdoc />
        public virtual bool OnCheckAvailable(TObject obj) => true;
    }
}