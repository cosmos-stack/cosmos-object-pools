using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for policy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPolicy<T> : IPolicy
    {
        /// <summary>
        /// On create event<br />
        /// 对象池的对象被创建时
        /// </summary>
        /// <returns>返回被创建的对象</returns>
        new T OnCreate();

        /// <summary>
        /// On destroy event<br />
        /// 销毁对象
        /// </summary>
        /// <param name="obj">资源对象</param>
        void OnDestroy(T obj);

        /// <summary>
        /// On get event<br />
        /// 从对象池获取对象成功的时候触发，通过该方法统计或初始化对象
        /// </summary>
        /// <param name="obj">资源对象</param>
        void OnGet(Object<T> obj);

        /// <summary>
        /// On get async event<br />
        /// 从对象池获取对象成功的时候触发，通过该方法统计或初始化对象
        /// </summary>
        /// <param name="obj">资源对象</param>
        Task OnGetAsync(Object<T> obj);

        /// <summary>
        /// On return event<br />
        /// 归还对象给对象池的时候触发
        /// </summary>
        /// <param name="obj">资源对象</param>
        void OnReturn(Object<T> obj);

        /// <summary>
        /// On check available event<br />
        /// 检查可用性
        /// </summary>
        /// <param name="obj">资源对象</param>
        /// <returns></returns>
        bool OnCheckAvailable(Object<T> obj);
    }
}