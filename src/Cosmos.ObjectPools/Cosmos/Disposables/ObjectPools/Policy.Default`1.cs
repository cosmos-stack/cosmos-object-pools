using System;
using System.Threading.Tasks;
using Cosmos.Asynchronous;
using Cosmos.Disposables.ObjectPools.Core;
using Cosmos.Reflection;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Default policy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultPolicy<T> : PolicyBase<T, ObjectOut<T>>, IPolicy<T>
    {
        /// <inheritdoc />
        public DefaultPolicy()
        {
            Name = Types.Of<DefaultPolicy<T>>().FullName;
        }

        /// <summary>
        /// Create object
        /// </summary>
        public Func<T> CreateObject;

        /// <summary>
        /// On get object
        /// </summary>
        public Action<ObjectOut<T>> OnGetObject;

        /// <inheritdoc />
        public override T OnCreate() => CreateObject();

        /// <inheritdoc />
        public override Task OnGetAsync(ObjectOut<T> obj)
        {
            OnGetObject?.Invoke(obj);
            return Tasks.CompletedTask();
        }
    }
}