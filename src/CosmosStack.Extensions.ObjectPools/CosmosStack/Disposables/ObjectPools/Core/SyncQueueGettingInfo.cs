using System.Threading;

namespace CosmosStack.Disposables.ObjectPools.Core
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

        internal ManualResetEventSlim Wait { get; set; } = new();

        internal TObjectOut ReturnValue { get; set; }

        internal object Lock = new();

        internal bool IsTimeout { get; set; }
    }
}