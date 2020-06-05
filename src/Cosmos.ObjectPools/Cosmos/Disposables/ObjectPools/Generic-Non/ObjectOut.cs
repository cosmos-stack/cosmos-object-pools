using System;
using System.Text;
using Cosmos.Disposables.ObjectPools.Core;

// ReSharper disable once CheckNamespace
namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Non-generic recyclable resource objects.<br />
    /// 非泛型可回收资源对象
    /// </summary>
    public class ObjectOut : ObjectOutBase<object>, IObject
    {
        /// <summary>
        /// Owning object pool<br />
        /// 所属对象池
        /// </summary>
        public IObjectPool Pool { get; internal set; }
        
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

        /// <inheritdoc />
        public override void ResetValue()
        {
            if (Value != null)
            {
                try
                {
                    Pool.Policy.OnDestroy(Value);
                }
                catch
                {
                    // ignored
                }

                try
                {
                    (Value as IDisposable)?.Dispose();
                }
                catch
                {
                    // ignored
                }
            }

            object value = default;

            try
            {
                value = Pool.Policy.OnCreate();
            }
            catch
            {
                // ignored
            }

            Value = value;
            LastReturnTime = DateTime.Now;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Pool?.Return(this);
        }
    }
}