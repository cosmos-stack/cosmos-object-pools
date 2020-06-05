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
    /// <typeparam name="T"></typeparam>
    public class DefaultPolicy<T> : DefaultPolicy, IPolicy<T>
    {
        /// <inheritdoc />
        public DefaultPolicy()
        {
            BindingType = typeof(T);
            Name = Types.Of<DefaultPolicy<T>>().FullName;
        }

        /// <summary>
        /// Create object
        /// </summary>
        public new Func<T> CreateObject;

        /// <summary>
        /// On get object
        /// </summary>
        public new Action<Object<T>> OnGetObject;

        /// <inheritdoc />
        public new T OnCreate() => CreateObject();

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
        public void OnReturn(Object<T> obj) { }

        /// <inheritdoc />
        public bool OnCheckAvailable(Object<T> obj) => true;
    }
}