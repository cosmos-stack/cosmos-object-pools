using System;
using CosmosStack.Disposables.ObjectPools.Statistics;
using CosmosStack.IdUtils;

namespace CosmosStack.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Base recyclable object
    /// </summary>
    public abstract class ObjectCell<T> : IObjectCell
    {
        /// <summary>
        /// Create a new instance of <see cref="ObjectCell{T}"/>
        /// </summary>
        protected ObjectCell()
        {
            InternalIdentity = RandomNonceStrGenerator.Create(32);
        }

        /// <summary>
        /// Create a new instance of <see cref="ObjectCell{T}"/>
        /// </summary>
        protected internal ObjectCell(string internalId, DynamicObjectCell dynamicObjectCell)
        {
            InternalIdentity = internalId;
            _dynamicObjectCell = dynamicObjectCell;
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
        private DynamicObjectCell _dynamicObjectCell;

        /// <summary>
        /// Value
        /// </summary>
        public T Value
        {
            get => _dynamicObjectCell.GetValue<T>();
            internal set => _dynamicObjectCell = new DynamicObjectCell(value, typeof(T));
        }

        /// <inheritdoc />
        public DynamicObjectCell GetDynamicObjectOut() => _dynamicObjectCell;

        internal void SetDynamicObjectOut(DynamicObjectCell dynamicObjectCell) => _dynamicObjectCell = dynamicObjectCell;

        #endregion

        #region Times

        /// <summary>
        /// Total times acquired
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal long _totalAcquiredTimes;

        /// <inheritdoc />
        public long TotalAcquiredTimes => _totalAcquiredTimes;

        #endregion

        #region Opt time and Id

        /// <inheritdoc />
        public DateTime LastAcquiredTime { get; internal set; }

        /// <inheritdoc />
        public DateTime LastRecycledTime { get; internal set; }

        /// <inheritdoc />
        public DateTime CreatedTime { get; } = DateTime.Now;

        /// <inheritdoc />
        public int LastAcquiredThreadId { get; internal set; }

        /// <inheritdoc />
        public int LastRecycledThreadId { get; internal set; }

        #endregion

        #region ResetValue

        /// <inheritdoc />
        public abstract void Reset();

        #endregion

        #region Recycle

        /// <summary>
        /// Is returned
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal bool _isRecycled = false;

        #endregion

        #region Dispose

        /// <inheritdoc />
        public abstract void Dispose();

        #endregion

        #region Statistics

        public ObjectBoxStatisticsInfo GetStatisticsInfo()
        {
            return new(
                $"{Value}",
                TotalAcquiredTimes,
                LastAcquiredThreadId,
                LastRecycledThreadId,
                LastAcquiredTime,
                LastRecycledTime);
        }

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
            return GetStatisticsInfo().ToString();
        }

        #endregion
    }
}