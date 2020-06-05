using System;
using System.Threading.Tasks;
using Cosmos.Asynchronous;
using Cosmos.Reflection;

// ReSharper disable once CheckNamespace
namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Default policy
    /// </summary>
    public class DefaultPolicy : IPolicy
    {
        /// <summary>
        /// Create a new instance of <see cref="DefaultPolicy"/>.
        /// </summary>
        public DefaultPolicy(Type bindingType)
        {
            BindingType = bindingType ?? throw new ArgumentNullException(nameof(bindingType));
            Name = $"{Types.Of<DefaultPolicy>().FullName}-{BindingType.FullName}";
        }

        internal DefaultPolicy() { }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public Type BindingType { get; set; }

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
        public Func<object> CreateObject;

        /// <summary>
        /// On get object
        /// </summary>
        public Action<ObjectOut> OnGetObject;

        /// <inheritdoc />
        public object OnCreate() => CreateObject();

        /// <inheritdoc />
        public void OnDestroy(object obj) { }

        /// <inheritdoc />
        public void OnGet(ObjectOut obj) { }

        /// <inheritdoc />
        public Task OnGetAsync(ObjectOut obj)
        {
            OnGetObject?.Invoke(obj);
            return Tasks.CompletedTask();
        }

        /// <inheritdoc />
        public void OnGetTimeout() { }

        /// <inheritdoc />
        public void OnReturn(ObjectOut obj) { }

        /// <inheritdoc />
        public bool OnCheckAvailable(ObjectOut obj) => true;

        /// <inheritdoc />
        public void OnAvailable() { }

        /// <inheritdoc />
        public void OnUnavailable() { }
    }
}