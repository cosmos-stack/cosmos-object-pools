using System.Collections.Generic;
using System.Text;

namespace Cosmos.Disposables.ObjectPools.Statistics
{
    public struct FullStatisticsInfo
    {
        public FullStatisticsInfo(ObjectPoolMode mode, StatisticsInfo statistics, IEnumerable<ObjectBoxStatisticsInfo> objectBoxStatisticsInfos)
        {
            Mode = mode;
            Statistics = statistics;
            ObjectBoxStatisticsInfos = objectBoxStatisticsInfos;
        }

        private ObjectPoolMode Mode { get; }

        private StatisticsInfo Statistics { get; }

        private IEnumerable<ObjectBoxStatisticsInfo> ObjectBoxStatisticsInfos { get; }

        public StringBuilder ToStringBuilder(StringBuilder builder)
        {
            builder ??= new StringBuilder();
            var buffer = new StringBuilder();

            builder.AppendLine($"Mode: {Mode}");
            builder.Append(Statistics.ToStringBuilder(buffer));
            builder.AppendLine().AppendLine();

            foreach (var info in ObjectBoxStatisticsInfos)
            {
                buffer.Clear();
                builder.Append(info.ToStringBuilder(buffer));
                builder.AppendLine();
            }

            return builder;
        }

        public StringBuilder ToStringBuilder()
        {
            return ToStringBuilder(new());
        }

        public override string ToString()
        {
            return ToStringBuilder().ToString();
        }
    }
}