using System;
using System.Text;

namespace CosmosStack.Disposables.ObjectPools.Statistics
{
    public struct ObjectBoxStatisticsInfo
    {
        internal ObjectBoxStatisticsInfo(
            string objStrVal,
            long totalAcquiredTimes,
            int lastAcquiredThreadId,
            int lastRecycledThreadId,
            DateTime lastAcquiredTime,
            DateTime lastRecycledTime
        )
        {
            ObjStrVal = objStrVal;
            TotalAcquiredTimes = totalAcquiredTimes;
            LastAcquiredThreadId = lastAcquiredThreadId;
            LastRecycledThreadId = lastRecycledThreadId;
            LastAcquiredTime = lastAcquiredTime;
            LastRecycledTime = lastRecycledTime;
        }

        private string ObjStrVal { get; }

        private long TotalAcquiredTimes { get; }

        private int LastAcquiredThreadId { get; }

        private int LastRecycledThreadId { get; }

        private DateTime LastAcquiredTime { get; }

        private DateTime LastRecycledTime { get; }

        public StringBuilder ToStringBuilder(StringBuilder builder)
        {
            builder ??= new StringBuilder();

            builder.Append($"{ObjStrVal}, ");
            builder.Append($"Times: {TotalAcquiredTimes}, ");
            builder.Append($"ThreadId(R/G): {LastRecycledThreadId}/{LastAcquiredThreadId}, ");
            builder.Append($"Time(R/G): {LastRecycledTime:yyyy-MM-dd HH:mm:ss:ms}/{LastAcquiredTime:yyyy-MM-dd HH:mm:ss:ms}");

            return builder;
        }

        public StringBuilder ToStringBuilder() => ToStringBuilder(new());

        public override string ToString() => ToStringBuilder().ToString();
    }
}