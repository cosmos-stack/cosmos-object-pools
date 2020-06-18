using System;
using System.Text;
using Cosmos.IdUtils;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Base recyclable object
    /// </summary>
    public abstract class ObjectOutBase<T> : IObjectOut
    {
        /// <summary>
        /// Create a new instance of <see cref="ObjectOutBase{T}"/>
        /// </summary>
        protected ObjectOutBase()
        {
            InternalIdentity = RandomNonceStrProvider.Create(32);
        }

        /// <summary>
        /// Create a new instance of <see cref="ObjectOutBase{T}"/>
        /// </summary>
        protected internal ObjectOutBase(string internalId, DynamicObjectOut dynamicObjectOut)
        {
            InternalIdentity = internalId;
            _dynamicObjectOut = dynamicObjectOut;
        }

        #region Outter Id and internal Id

        /// <summary>
        /// Internal identity
        /// </summary>
        internal string InternalIdentity { get; }

        /// <inheritdoc />
        public int Id { get; internal set; }

        #endregion

        #region Value

        /// <summary>
        /// Dynamic object out
        /// </summary>
        private DynamicObjectOut _dynamicObjectOut;

        /// <summary>
        /// Value
        /// </summary>
        public T Value
        {
            get => _dynamicObjectOut.GetValue<T>();
            internal set => _dynamicObjectOut = new DynamicObjectOut(value, typeof(T));
        }

        /// <inheritdoc />
        public DynamicObjectOut GetDynamicObjectOut() => _dynamicObjectOut;

        internal void SetDynamicObjectOut(DynamicObjectOut dynamicObjectOut) => _dynamicObjectOut = dynamicObjectOut;

        #endregion

        #region Times

        /// <summary>
        /// Total times acquired
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal long _getTimes;

        /// <inheritdoc />
        public long GetTimes => _getTimes;

        #endregion

        #region Opt time and Id

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

        #endregion

        #region ResetValue

        /// <inheritdoc />
        public abstract void ResetValue();

        #endregion

        #region Return

        /// <summary>
        /// Is returned
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal bool _isReturned = false;

        #endregion

        #region Dispose

        /// <inheritdoc />
        public abstract void Dispose();

        #endregion

        #region GetHashCode and ToString

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return InternalIdentity.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Value}, ");
            sb.Append($"Times: {GetTimes}, ");
            sb.Append($"ThreadId(R/G): {LastReturnThreadId}/{LastGetThreadId}, ");
            sb.Append($"Time(R/G): {LastReturnTime:yyyy-MM-dd HH:mm:ss:ms}/{LastGetTime:yyyy-MM-dd HH:mm:ss:ms}");

            return sb.ToString();
        }

        #endregion
    }
}