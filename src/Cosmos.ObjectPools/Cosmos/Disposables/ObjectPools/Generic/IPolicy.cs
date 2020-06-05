using Cosmos.Disposables.ObjectPools.Core;

// ReSharper disable once CheckNamespace

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for policy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPolicy<T> : IPolicyCore<T, ObjectOut<T>> { }
}