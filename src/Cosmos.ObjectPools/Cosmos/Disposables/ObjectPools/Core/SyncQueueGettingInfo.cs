using System.Threading;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Synchronous getting queue info
    /// </summary>
    /// <typeparam name="TObjectOut"></typeparam>
    public class SyncQueueGettingInfo<TObjectOut> : DisposableObjects
    {
        /// <inheritdoc />
        public SyncQueueGettingInfo()
        {
            AddDisposableAction("releaseSelf", () =>
            {
                try
                {
                    Wait?.Dispose();
                }
                catch
                {
                    // ignored
                }
            });
        }

        internal ManualResetEventSlim Wait { get; set; } = new ManualResetEventSlim();

        internal TObjectOut ReturnValue { get; set; }

        internal object Lock = new object();

        internal bool IsTimeout { get; set; }
    }
}