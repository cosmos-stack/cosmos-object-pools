using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Object pool base
    /// </summary>
    public abstract class ObjectPoolBase<T, TPolicy, TObject> : IObjectPoolCore<TPolicy>
        where TPolicy : IPolicyCore<T, TObject>
        where TObject : ObjectOutBase<T>, IObjectOut, new()
    {
        /// <summary>
        /// Create a new instance of <see cref="ObjectPoolBase{T, TPolicy, TObject}"/>.<br />
        /// 创建对象池
        /// </summary>
        /// <param name="policy"></param>
        protected ObjectPoolBase(TPolicy policy)
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

        private readonly List<TObject> _allObjects = new List<TObject>();
        private readonly object _allObjectsLockObj = new object();

        private readonly ConcurrentStack<TObject> _freeObjects = new ConcurrentStack<TObject>();

        private readonly ConcurrentQueue<bool> _getQueue = new ConcurrentQueue<bool>();
        private readonly ConcurrentQueue<SyncQueueGettingInfo<TObject>> _getSyncQueue = new ConcurrentQueue<SyncQueueGettingInfo<TObject>>();
        private readonly ConcurrentQueue<TaskCompletionSource<TObject>> _getAsyncQueue = new ConcurrentQueue<TaskCompletionSource<TObject>>();

        /// <summary>
        /// Gets policy
        /// </summary>
        public TPolicy Policy { get; protected set; }

        /// <summary>
        /// Object pool mode
        /// </summary>
        public abstract ObjectPoolMode Mode { get; }

        #region Available and unavailable

        private readonly object _unavailableLockObj = new object();
        private bool _running = true;

        /// <inheritdoc />
        public virtual bool IsAvailable => UnavailableException is null;

        /// <inheritdoc />
        public Exception UnavailableException { get; protected set; }

        /// <inheritdoc />
        public DateTime? UnavailableTime { get; protected set; }

        /// <inheritdoc />
        public virtual bool SetUnavailable(Exception exception)
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
        public string Statistics
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
        public string StatisticsFully
        {
            get
            {
                var sb = new StringBuilder();

                sb.AppendLine($"Mode: {Mode}");
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

        #region Get or return

        /// <summary>
        /// A factory method of creating an instance of &lt;TObject&gt;
        /// </summary>
        /// <returns></returns>
        protected abstract Func<int, TObject> RecyclableObjectFactory();

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private TObject GetOrCreateFreeObject(bool checkAvailable)
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
                        _allObjects.Add(obj = RecyclableObjectFactory()(_allObjects.Count));
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

        /// <summary>
        /// Access to resources.<br />
        /// 获取资源
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual TObject Get(TimeSpan? timeout = null)
        {
            // Get resources
            var obj = GetOrCreateFreeObject(true);

            if (obj is null)
            {
                var queueItem = new SyncQueueGettingInfo<TObject>();

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

        /// <summary>
        /// Access to resources async.<br />
        /// 获取资源
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<TObject> GetAsync()
        {
            var obj = GetOrCreateFreeObject(true);

            if (obj is null)
            {
                if (Policy.AsyncGetCapacity > 0 && _getAsyncQueue.Count >= Policy.AsyncGetCapacity - 1)
                    throw ExceptionNew.NoResourcesAvailableForAsynchronousCalls(Policy.AsyncGetCapacity);

                var tcs = new TaskCompletionSource<TObject>();

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

        /// <summary>
        /// Return the resource after use.<br />
        /// 使用完毕后，归还资源
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="isReset">是否重新创建</param>
        public virtual void Return(TObject obj, bool isReset = false)
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

        #endregion

        #region Dispose

        /// <inheritdoc />
        public virtual void Dispose()
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

        #endregion
    }
}