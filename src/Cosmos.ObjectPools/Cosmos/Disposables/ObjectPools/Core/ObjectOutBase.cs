using System;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Base recyclable object
    /// </summary>
    public abstract class ObjectOutBase<T> : IObjectOut
    {
        /// <inheritdoc />
        public int Id { get; internal set; }

        /// <summary>
        /// Value
        /// </summary>
        public T Value { get; internal set; }

        /// <summary>
        /// Total times acquired
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal long _getTimes;

        /// <inheritdoc />
        public long GetTimes => _getTimes;

        /// <inheritdoc />
        public DateTime LastGetTime { get; internal set; }

        /// <inheritdoc />
        public DateTime LastReturnTime { get; internal set; }

        /// <inheritdoc />
        public DateTime CreateTime { get; internal set; } = DateTime.Now;

        /// <inheritdoc />
        public int LastGetThreadId { get; internal set; }

        /// <inheritdoc />
        public int LastReturnThreadId { get; internal set; }

        /// <inheritdoc />
        public abstract void ResetValue();

        /// <summary>
        /// Is returned
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal bool _isReturned = false;

        /// <inheritdoc />
        public abstract void Dispose();
    }
}