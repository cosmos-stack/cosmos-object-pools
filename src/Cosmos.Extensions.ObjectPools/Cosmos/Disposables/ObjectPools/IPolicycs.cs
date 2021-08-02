using System;
using Cosmos.Disposables.ObjectPools.Core;

namespace Cosmos.Disposables.ObjectPools
{
    /// <summary>
    /// Interface for policy
    /// </summary>
    public interface IPolicy : IPolicyCore<object, ObjectPayload>
    {
        /// <summary>
        /// Binding type<br />
        /// 绑定的类型
        /// </summary>
        Type BindingType { get; set; }
    }
}