using System;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Object pool base
    /// </summary>
    public abstract class ObjectPoolBase : IObjectPoolCore
    {
        /// <inheritdoc />
        public virtual bool IsAvailable => UnavailableException is null;

        /// <inheritdoc />
        public Exception UnavailableException { get; protected set; }

        /// <inheritdoc />
        public DateTime? UnavailableTime { get; protected set; }

        /// <inheritdoc />
        public abstract bool SetUnavailable(Exception exception);

        /// <inheritdoc />
        public abstract string Statistics { get; }

        /// <inheritdoc />
        public abstract string StatisticsFully { get; }

        /// <inheritdoc />
        public abstract void Dispose();
    }
}