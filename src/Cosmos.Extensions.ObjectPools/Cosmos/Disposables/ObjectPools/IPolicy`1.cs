using Cosmos.Disposables.ObjectPools.Core;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for policy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPolicy<T> : IPolicyCore<T, ObjectPayload<T>> { }
}