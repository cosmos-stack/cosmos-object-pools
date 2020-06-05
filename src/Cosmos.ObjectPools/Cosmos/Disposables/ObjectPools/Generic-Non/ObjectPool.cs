using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Disposables.ObjectPools.Core;

// ReSharper disable once CheckNamespace
namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Non-generic Object pool<br />
    /// 对象池
    /// </summary>
    public class ObjectPool : ObjectPoolBase, IObjectPool
    {
        /// <inheritdoc />
        public IPolicy Policy { get; protected set; }

        private readonly List<ObjectOut> _allObjects = new List<ObjectOut>();
        private readonly object _allObjectsLockObj = new object();

        private readonly ConcurrentStack<ObjectOut> _freeObjects = new ConcurrentStack<ObjectOut>();

        private readonly ConcurrentQueue<bool> _getQueue = new ConcurrentQueue<bool>();
        private readonly ConcurrentQueue<GetSyncQueueInfo> _getSyncQueue = new ConcurrentQueue<GetSyncQueueInfo>();
        private readonly ConcurrentQueue<TaskCompletionSource<ObjectOut>> _getAsyncQueue = new ConcurrentQueue<TaskCompletionSource<ObjectOut>>();

        /// <summary>
        /// Create a new instance of <see cref="ObjectPool{T}"/>.<br />
        /// 创建对象池
        /// </summary>
        /// <param name="bindingType">绑定的类型</param>
        /// <param name="poolSize">池大小</param>
        /// <param name="createObject">池内对象的创建委托</param>
        /// <param name="onGetObject">获取池内对象成功后，进行使用前操作</param>
        public ObjectPool(Type bindingType, int poolSize, Func<object> createObject, Action<ObjectOut> onGetObject = null)
            : this(new DefaultPolicy(bindingType) {PoolSize = poolSize, CreateObject = createObject, OnGetObject = onGetObject}) { }

        /// <summary>
        /// Create a new instance of <see cref="ObjectPool{T}"/>.<br />
        /// 创建对象池
        /// </summary>
        /// <param name="policy">策略</param>
        public ObjectPool(IPolicy policy)
        {
            Policy = policy;

            AppDomain.CurrentDomain.ProcessExit += (s1, e1) =>
            {
                if (Policy.IsAutoDisposeWithSystem)
                    _running = false;
            };
            try
            {
                Console.CancelKeyPress += (s1, e1) =>
                {
                    if (e1.Cancel) return;
                    if (Policy.IsAutoDisposeWithSystem)
                        _running = false;
                };
            }
            catch
            {
                // ignored
            }
        }

        internal ObjectPool() { }

        #region Available and unavailable

        private readonly object _unavailableLockObj = new object();
        private bool _running = true;

        /// <inheritdoc />
        public override bool SetUnavailable(Exception exception)
        {
            var hasSet = false;

            if (exception != null && UnavailableException is null)
            {
                lock (_unavailableLockObj)
                {
                    if (UnavailableException is null)
                    {
                        UnavailableException = exception;
                        UnavailableTime = DateTime.Now;
                        hasSet = true;
                    }
                }
            }

            if (hasSet)
            {
                Policy.OnUnavailable();
                CheckAvailable(Policy.CheckAvailableInterval);
            }

            return hasSet;
        }

        /// <summary>
        /// Check availability regularly in the background.<br />
        /// 后台定时检查可用性
        /// </summary>
        /// <param name="interval"></param>
        private void CheckAvailable(int interval)
        {
            new Thread(() =>
            {
                if (UnavailableException != null)
                {
                    Displayer.Unavailable($"【{Policy.Name}】恢复检查时间：{DateTime.Now.AddSeconds(interval)}");
                }

                while (UnavailableException != null)
                {
                    if (_running == false) return;

                    Thread.CurrentThread.Join(TimeSpan.FromSeconds(interval));

                    if (_running == false) return;

                    try
                    {
                        var conn = GetOrCreateFreeObject(false);
                        if (conn is null)
                            throw ExceptionNew.CA_UnableToObtainResources(Statistics);

                        try
                        {
                            if (Policy.OnCheckAvailable(conn) == false)
                                throw ExceptionNew.CA_StillUnableToObtainResources();

                            break;
                        }
                        finally
                        {
                            Return(conn);
                        }
                    }
                    catch (Exception ex)
                    {
                        Displayer.Unavailable($"【{Policy.Name}】仍然不可用，下一次恢复检查时间：{DateTime.Now.AddSeconds(interval)}，错误：({ex.Message})");
                    }
                }

                RestoreToAvailable();
            }).Start();
        }

        /// <summary>
        /// Restore to available<br />
        /// 恢复为可用
        /// </summary>
        private void RestoreToAvailable()
        {
            bool isRestored = false;
            if (UnavailableException != null)
            {
                lock (_unavailableLockObj)
                {
                    if (UnavailableException != null)
                    {
                        UnavailableException = null;
                        UnavailableTime = null;
                        isRestored = true;
                    }
                }
            }

            if (isRestored)
            {
                lock (_allObjectsLockObj)
                    _allObjects.ForEach(a => a.LastGetTime = a.LastReturnTime = new DateTime(2000, 1, 1));

                Policy.OnAvailable();

                Displayer.Available($"【{Policy.Name}】已恢复工作");
            }
        }

        /// <summary>
        /// Live check available
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected bool LiveCheckAvailable()
        {
            try
            {
                var conn = GetOrCreateFreeObject(false);
                if (conn is null)
                    throw ExceptionNew.LCA_UnableToObtainResources(Statistics);

                try
                {
                    if (Policy.OnCheckAvailable(conn) == false)
                        throw ExceptionNew.LCA_StillUnableToObtainResources();
                }
                finally
                {
                    Return(conn);
                }
            }
            catch
            {
                return false;
            }

            RestoreToAvailable();

            return true;
        }

        #endregion

        #region Statistics

        /// <inheritdoc />
        public override string Statistics
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append($"Pool: {_freeObjects.Count}/{_allObjects.Count}, ");
                sb.Append($"Get wait: {_getSyncQueue.Count}, ");
                sb.Append($"GetAsync wait: {_getAsyncQueue.Count}");
                return sb.ToString();
            }
        }

        /// <inheritdoc />
        public override string StatisticsFully
        {
            get
            {
                var sb = new StringBuilder();

                sb.AppendLine(Statistics);
                sb.AppendLine("");

                foreach (var obj in _allObjects)
                {
                    sb.Append($"{obj.Value}, ");
                    sb.Append($"Times: {obj.GetTimes}, ");
                    sb.Append($"ThreadId(R/G): {obj.LastReturnThreadId}/{obj.LastGetThreadId}, ");
                    sb.Append($"Time(R/G): {obj.LastReturnTime:yyyy-MM-dd HH:mm:ss:ms}/{obj.LastGetTime:yyyy-MM-dd HH:mm:ss:ms}");
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }

        #endregion

        /// <summary>
        /// Get or create available resources<br />
        /// 获取可用资源，或创建资源
        /// </summary>
        /// <param name="checkAvailable"></param>
        /// <returns></returns>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private ObjectOut GetOrCreateFreeObject(bool checkAvailable)
        {
            // Object pool has been released and cannot be accessed.
            if (_running == false)
                throw ExceptionNew.ObjectPolHasBeenReleased(Policy.Name);

            // Status is not available
            if (checkAvailable && UnavailableException != null)
                throw ExceptionNew.StatusIsNotAvailable(Policy.Name, UnavailableException?.Message);

            // When no available resources are obtained, or the resources are null
            // and the total number of objects does not exceed the upper limit of the resource pool,
            // a new resource is created and used as an available resource.
            if ((_freeObjects.TryPop(out var obj) == false || obj is null) && _allObjects.Count < Policy.PoolSize)
            {
                lock (_allObjectsLockObj)
                    if (_allObjects.Count < Policy.PoolSize)
                        _allObjects.Add(obj = new ObjectOut {Pool = this, Id = _allObjects.Count + 1});
            }

            // If the resource object is not empty at this time, it is marked as unreturned. Prepare to lend the resource.
            if (obj != null)
                obj._isReturned = false;

            // If the resource object is not empty at this time, but the value is empty (indicating that it has been Disposed);
            // or the resource object is not empty, but the idle time exceeds the value configured by the policy,
            // it will be reset.
            if (obj != null && obj.Value is null ||
                obj != null && Policy.IdleTimeout > TimeSpan.Zero && DateTime.Now.Subtract(obj.LastReturnTime) > Policy.IdleTimeout)
            {
                try
                {
                    obj.ResetValue();
                }
                catch
                {
                    Return(obj);
                    throw;
                }
            }

            // Lend the resource object.
            return obj;
        }

        /// <inheritdoc />
        public ObjectOut Get(TimeSpan? timeout = null)
        {
            // Get resources
            var obj = GetOrCreateFreeObject(true);

            if (obj is null)
            {
                var queueItem = new GetSyncQueueInfo();

                _getSyncQueue.Enqueue(queueItem);
                _getQueue.Enqueue(false);

                if (timeout is null)
                    timeout = Policy.SyncGetTimeout;

                try
                {
                    if (queueItem.Wait.Wait(timeout.Value))
                        obj = queueItem.ReturnValue;
                }
                catch
                {
                    // ignored
                }

                if (obj is null)
                    obj = queueItem.ReturnValue;

                if (obj is null)
                    lock (queueItem.Lock)
                        queueItem.IsTimeout = (obj = queueItem.ReturnValue) is null;

                if (obj is null)
                    obj = queueItem.ReturnValue;

                if (obj is null)
                {
                    Policy.OnGetTimeout();

                    if (Policy.IsThrowGetTimeoutException)
                        throw ExceptionNew.ResourceAcquisitionTimeout(timeout.Value.TotalSeconds);

                    return null;
                }
            }

            try
            {
                Policy.OnGet(obj);
            }
            catch
            {
                Return(obj);
                throw;
            }

            obj.LastGetThreadId = Thread.CurrentThread.ManagedThreadId;
            obj.LastGetTime = DateTime.Now;
            Interlocked.Increment(ref obj._getTimes);

            return obj;
        }

        /// <inheritdoc />
        public async Task<ObjectOut> GetAsync()
        {
            var obj = GetOrCreateFreeObject(true);

            if (obj is null)
            {
                if (Policy.AsyncGetCapacity > 0 && _getAsyncQueue.Count >= Policy.AsyncGetCapacity - 1)
                    throw ExceptionNew.NoResourcesAvailableForAsynchronousCalls(Policy.AsyncGetCapacity);

                var tcs = new TaskCompletionSource<ObjectOut>();

                _getAsyncQueue.Enqueue(tcs);
                _getQueue.Enqueue(true);

                obj = await tcs.Task;

                //if (timeout is null) timeout = Policy.SyncGetTimeout;

                //if (tcs.Task.Wait(timeout.Value))
                //	obj = tcs.Task.Result;

                //if (obj is null) {

                //	tcs.TrySetCanceled();
                //	Policy.GetTimeout();

                //	if (Policy.IsThrowGetTimeoutException)
                //		throw new Exception($"ObjectPool.GetAsync 获取超时（{timeout.Value.TotalSeconds}秒）。");

                //	return null;
                //}
            }

            try
            {
                await Policy.OnGetAsync(obj);
            }
            catch
            {
                Return(obj);
                throw;
            }

            obj.LastGetThreadId = Thread.CurrentThread.ManagedThreadId;
            obj.LastGetTime = DateTime.Now;
            Interlocked.Increment(ref obj._getTimes);

            return obj;
        }

        /// <inheritdoc />
        public void Return(ObjectOut obj, bool isReset = false)
        {
            if (obj is null) return;

            if (obj._isReturned) return;

            if (_running == false)
            {
                Policy.OnDestroy(obj.Value);
                try
                {
                    (obj.Value as IDisposable)?.Dispose();
                }
                catch
                {
                    // ignored
                }

                return;
            }

            if (isReset) obj.ResetValue();

            var isReturn = false;

            while (isReturn == false && _getQueue.TryDequeue(out var isAsync))
            {
                if (isAsync == false)
                {
                    if (_getSyncQueue.TryDequeue(out var queueItem) && queueItem != null)
                    {
                        lock (queueItem.Lock)
                            if (queueItem.IsTimeout == false)
                                queueItem.ReturnValue = obj;

                        if (queueItem.ReturnValue != null)
                        {
                            obj.LastReturnThreadId = Thread.CurrentThread.ManagedThreadId;
                            obj.LastReturnTime = DateTime.Now;

                            try
                            {
                                queueItem.Wait.Set();
                                isReturn = true;
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        try
                        {
                            queueItem.Dispose();
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                else
                {
                    if (_getAsyncQueue.TryDequeue(out var tcs) && tcs != null && tcs.Task.IsCanceled == false)
                    {
                        obj.LastReturnThreadId = Thread.CurrentThread.ManagedThreadId;
                        obj.LastReturnTime = DateTime.Now;

                        try
                        {
                            isReturn = tcs.TrySetResult(obj);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }

            //无排队，直接归还
            if (isReturn == false)
            {
                try
                {
                    Policy.OnReturn(obj);
                }
                finally
                {
                    obj.LastReturnThreadId = Thread.CurrentThread.ManagedThreadId;
                    obj.LastReturnTime = DateTime.Now;
                    obj._isReturned = true;

                    _freeObjects.Push(obj);
                }
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _running = false;

            while (_freeObjects.TryPop(out _)) { }

            while (_getSyncQueue.TryDequeue(out var sync))
            {
                try
                {
                    sync.Wait.Set();
                }
                catch
                {
                    // ignored
                }
            }

            while (_getAsyncQueue.TryDequeue(out var async))
                async.TrySetCanceled();

            while (_getQueue.TryDequeue(out _)) { }

            for (var i = 0; i < _allObjects.Count; i++)
            {
                Policy.OnDestroy(_allObjects[i].Value);
                try
                {
                    (_allObjects[i].Value as IDisposable)?.Dispose();
                }
                catch
                {
                    // ignored
                }
            }

            _allObjects.Clear();
        }

        /// <summary>
        /// Get sync queue info
        /// </summary>
        class GetSyncQueueInfo : DisposableObjects
        {
            public GetSyncQueueInfo()
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

            internal ObjectOut ReturnValue { get; set; }

            internal object Lock = new object();

            internal bool IsTimeout { get; set; }
        }
    }
}