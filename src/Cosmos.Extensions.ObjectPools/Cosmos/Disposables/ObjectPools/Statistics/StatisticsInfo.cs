using System.Text;

namespace Cosmos.Disposables.ObjectPools.Statistics
{
    public struct StatisticsInfo
    {
        internal StatisticsInfo(
            int countOfFreeObjects,
            int countOfAllObjects,
            int itemsInSyncQueue,
            int itemsInAsyncQueue)
        {
            CountOfFreeObjects = countOfFreeObjects;
            CountOfAllObjects = countOfAllObjects;
            ItemsInSyncQueue = itemsInSyncQueue;
            ItemsInAsyncQueue = itemsInAsyncQueue;
        }

        private int CountOfFreeObjects { get; }

        private int CountOfAllObjects { get; }

        private int ItemsInSyncQueue { get; }

        private int ItemsInAsyncQueue { get; }

        public StringBuilder ToStringBuilder(StringBuilder builder)
        {
            builder ??= new StringBuilder();

            builder.Append($"Pool: {CountOfFreeObjects}/{CountOfAllObjects}, ");
            builder.Append($"Get wait: {ItemsInSyncQueue}, ");
            builder.Append($"GetAsync wait: {ItemsInAsyncQueue}");

            return builder;
        }

        public StringBuilder ToStringBuilder() => ToStringBuilder(new());

        public override string ToString() => ToStringBuilder().ToString();

        public static implicit operator string(StatisticsInfo statistics)
        {
            return statistics.ToString();
        }
    }
}