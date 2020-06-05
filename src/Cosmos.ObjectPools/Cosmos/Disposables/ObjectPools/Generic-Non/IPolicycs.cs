using System;
using System.Threading.Tasks;
using Cosmos.Disposables.ObjectPools.Core;

// ReSharper disable once CheckNamespace
namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for policy
    /// </summary>
    public interface IPolicy : IPolicyCore
    {
        /// <summary>
        /// Binding type<br />
        /// 绑定的类型
        /// </summary>
        Type BindingType { get; set; }
        
        /// <summary>
        /// On create event<br />
        /// 对象池的对象被创建时
        /// </summary>
        /// <returns>返回被创建的对象</returns>
        object OnCreate();

        /// <summary>
        /// On destroy event<br />
        /// 销毁对象
        /// </summary>
        /// <param name="obj">资源对象</param>
        void OnDestroy(object obj);

        /// <summary>
        /// On get event<br />
        /// 从对象池获取对象成功的时候触发，通过该方法统计或初始化对象
        /// </summary>
        /// <param name="obj">资源对象</param>
        void OnGet(ObjectOut obj);

        /// <summary>
        /// On get async event<br />
        /// 从对象池获取对象成功的时候触发，通过该方法统计或初始化对象
        /// </summary>
        /// <param name="obj">资源对象</param>
        Task OnGetAsync(ObjectOut obj);

        /// <summary>
        /// On return event<br />
        /// 归还对象给对象池的时候触发
        /// </summary>
        /// <param name="obj">资源对象</param>
        void OnReturn(ObjectOut obj);

        /// <summary>
        /// On check available event<br />
        /// 检查可用性
        /// </summary>
        /// <param name="obj">资源对象</param>
        /// <returns></returns>
        bool OnCheckAvailable(ObjectOut obj);
    }
}