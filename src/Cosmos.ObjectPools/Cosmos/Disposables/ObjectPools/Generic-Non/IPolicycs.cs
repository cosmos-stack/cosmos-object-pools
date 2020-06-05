using System;
using Cosmos.Disposables.ObjectPools.Core;

// ReSharper disable once CheckNamespace
namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for policy
    /// </summary>
    public interface IPolicy : IPolicyCore<object, ObjectOut>
    {
        /// <summary>
        /// Binding type<br />
        /// 绑定的类型
        /// </summary>
        Type BindingType { get; set; }
    }
}