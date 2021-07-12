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
    public class DefaultPolicy<T> : PolicyBase<T, ObjectCellSite<T>>, IPolicy<T>
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
        public Action<ObjectCellSite<T>> OnGetObject;

        /// <inheritdoc />
        public override T OnCreate() => CreateObject();

        /// <inheritdoc />
        public override Task OnAcquireAsync(ObjectCellSite<T> obj)
        {
            OnGetObject?.Invoke(obj);
            return Tasks.CompletedTask();
        }
    }
}