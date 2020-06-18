using System;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Dynamic object out
    /// </summary>
    public readonly struct DynamicObjectOut
    {
        private readonly dynamic _dynamicValue;
        private readonly Type _type;

        /// <summary>
        /// Create an instance of <see cref="DynamicObjectOut"/>.
        /// </summary>
        /// <param name="dynamicValue"></param>
        /// <param name="type"></param>
        public DynamicObjectOut(in dynamic dynamicValue, in Type type)
        {
            _dynamicValue = dynamicValue;
            _type = type;
        }

        /// <summary>
        /// Get dynamic value
        /// </summary>
        public dynamic Value => _dynamicValue;

        /// <summary>
        /// Get non-generic value
        /// </summary>
        /// <returns></returns>
        public object GetValue() => _dynamicValue;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetValue<T>() => (T) _dynamicValue;
        
        /// <summary>
        /// Is type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsType(in Type type) => _type == type;

        /// <summary>
        /// Is type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsType<T>() => typeof(T) == _type;

        /// <summary>
        /// Try get value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(out object value)
        {
            value = _dynamicValue;
            return true;
        }

        /// <summary>
        /// Try get value
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetValue<T>(out T value)
        {
            try
            {
                value = (T) _dynamicValue;
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }
    }
}