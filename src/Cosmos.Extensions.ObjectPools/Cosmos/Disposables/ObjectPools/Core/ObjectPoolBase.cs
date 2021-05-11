using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cosmos.Disposables.ObjectPools.Core.Display;
using Cosmos.Disposables.ObjectPools.Statistics;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Object pool base
    /// </summary>
    public abstract class ObjectPoolBase<T, TPolicy, TObject> : IObjectPoolCore<TPolicy>
        where TPolicy : IPolicyCore<T, TObject>
        where TObject : ObjectBoxBase<T>, IObjectBox, new()
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

        private readonly List<TObject> _allObjects = new();
        private readonly object _allObjectsLockObj = new();

        private readonly ConcurrentStack<TObject> _freeObjects = new();

        private readonly ConcurrentQueue<bool> _getQueue = new();
        private readonly ConcurrentQueue<SyncQueueGettingInfo<TObject>> _getSyncQueue = new();
        private readonly ConcurrentQueue<TaskCompletionSource<TObject>> _getAsyncQueue = new();

        /// <summary>
        /// Gets policy
        /// </summary>
        public TPolicy Policy { get; protected set; }

        /// <summary>
        /// Object pool mode
        /// </summary>
        public abstract ObjectPoolMode Mode { get; }

        #region Available and unavailable

        private readonly object _unavailableLockObj = new();
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
                    ConsoleWriter.Unavailable($"【{Policy.Name}】恢复检查时间：{DateTime.Now.AddSeconds(interval)}");
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
                            throw ExceptionHelper.CA_UnableToObtainResources(GetStatisticsInfo());

                        try
                        {
                            if (Policy.OnCheckAvailable(conn) == false)
                                throw ExceptionHelper.CA_StillUnableToObtainResources();

                            break;
                        }
                        finally
                        {
                            Return(conn);
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleWriter.Unavailable($"【{Policy.Name}】仍然不可用，下一次恢复检查时间：{DateTime.Now.AddSeconds(interval)}，错误：({ex.Message})");
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
                    _allObjects.ForEach(a => a.LastAcquiredTime = a.LastRecycledTime = new DateTime(2000, 1, 1));

                Policy.OnAvailable();

                ConsoleWriter.Available($"【{Policy.Name}】已恢复工作");
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
                    throw ExceptionHelper.LCA_UnableToObtainResources(GetStatisticsInfo());

                try
                {
                    if (Policy.OnCheckAvailable(conn) == false)
                        throw ExceptionHelper.LCA_StillUnableToObtainResources();
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
        public StatisticsInfo GetStatisticsInfo()
        {
            return new(
                _freeObjects.Count,
                _allObjects.Count,
                _getSyncQueue.Count,
                _getAsyncQueue.Count
            );
        }

        /// <inheritdoc />
        public FullStatisticsInfo GetStatisticsInfoFully()
        {
            return new(
                Mode,
                GetStatisticsInfo(),
                _allObjects.Select(x => x.GetStatisticsInfo()));
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
                throw ExceptionHelper.ObjectPolHasBeenReleased(Policy.Name);

            // Status is not available
            if (checkAvailable && UnavailableException != null)
                throw ExceptionHelper.StatusIsNotAvailable(Policy.Name, UnavailableException?.Message);

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
                obj._isRecycled = false;

            // If the resource object is not empty at this time, but the value is empty (indicating that it has been Disposed);
            // or the resource object is not empty, but the idle time exceeds the value configured by the policy,
            // it will be reset.
            if (obj != null && obj.Value is null ||
                obj != null && Policy.IdleTimeout > TimeSpan.Zero && DateTime.Now.Subtract(obj.LastRecycledTime) > Policy.IdleTimeout)
            {
                try
                {
                    obj.Reset();
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
                        throw ExceptionHelper.ResourceAcquisitionTimeout(timeout.Value.TotalSeconds);

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

            obj.LastAcquiredThreadId = Thread.CurrentThread.ManagedThreadId;
            obj.LastAcquiredTime = DateTime.Now;
            Interlocked.Increment(ref obj._totalAcquiredTimes);

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
                    throw ExceptionHelper.NoResourcesAvailableForAsynchronousCalls(Policy.AsyncGetCapacity);

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

            obj.LastAcquiredThreadId = Thread.CurrentThread.ManagedThreadId;
            obj.LastAcquiredTime = DateTime.Now;
            Interlocked.Increment(ref obj._totalAcquiredTimes);

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

            if (obj._isRecycled) return;

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

            if (isReset) obj.Reset();

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
                            obj.LastRecycledThreadId = Thread.CurrentThread.ManagedThreadId;
                            obj.LastRecycledTime = DateTime.Now;

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
                        obj.LastRecycledThreadId = Thread.CurrentThread.ManagedThreadId;
                        obj.LastRecycledTime = DateTime.Now;

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
                    obj.LastRecycledThreadId = Thread.CurrentThread.ManagedThreadId;
                    obj.LastRecycledTime = DateTime.Now;
                    obj._isRecycled = true;

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