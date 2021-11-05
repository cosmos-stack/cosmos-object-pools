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
    public class DefaultPolicy : PolicyBase<object, ObjectPayload>, IPolicy
    {
        /// <summary>
        /// Create a new instance of <see cref="DefaultPolicy"/>.
        /// </summary>
        public DefaultPolicy(Type bindingType)
        {
            BindingType = bindingType ?? throw new ArgumentNullException(nameof(bindingType));
            Name = $"{Types.Of<DefaultPolicy>().FullName}-{BindingType.FullName}";
        }

        /// <inheritdoc />
        public Type BindingType { get; set; }

        /// <summary>
        /// Create object
        /// </summary>
        public Func<object> CreateObject;

        /// <summary>
        /// On get object
        /// </summary>
        public Action<ObjectPayload> OnGetObject;

        /// <inheritdoc />
        public override object OnCreate() => CreateObject();

        /// <inheritdoc />
        public override Task OnAcquireAsync(ObjectPayload obj)
        {
            OnGetObject?.Invoke(obj);
            return Tasks.CompletedTask();
        }
    }
}