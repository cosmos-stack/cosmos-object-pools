using CosmosStack.Disposables.ObjectPools.Core;

namespace CosmosStack.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for policy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPolicy<T> : IPolicyCore<T, ObjectPayload<T>> { }
}