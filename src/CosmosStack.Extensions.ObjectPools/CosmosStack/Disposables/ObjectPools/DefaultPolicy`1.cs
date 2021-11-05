using System;
using System.Threading.Tasks;
using CosmosStack.Asynchronous;
using CosmosStack.Disposables.ObjectPools.Core;
using CosmosStack.Reflection;

namespace CosmosStack.Disposables.ObjectPools
{
    /// <summary>
    /// Default policy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultPolicy<T> : PolicyBase<T, ObjectPayload<T>>, IPolicy<T>
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
        public Action<ObjectPayload<T>> OnGetObject;

        /// <inheritdoc />
        public override T OnCreate() => CreateObject();

        /// <inheritdoc />
        public override Task OnAcquireAsync(ObjectPayload<T> obj)
        {
            OnGetObject?.Invoke(obj);
            return Tasks.CompletedTask();
        }
    }
}