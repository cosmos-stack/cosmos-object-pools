using System;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Exception helper
    /// </summary>
    internal static class ExceptionNew
    {
        public static Exception CA_UnableToObtainResources(string statistics)
        {
            return new Exception($"CheckAvailable: unable to obtain resource. {statistics}");
        }

        public static Exception CA_StillUnableToObtainResources()
        {
            return new Exception("CheckAvailable: Resources are still unavailable.");
        }

        public static Exception LCA_UnableToObtainResources(string statistics)
        {
            return new Exception($"LiveCheckAvailable: unable to obtain resource. {statistics}");
        }

        public static Exception LCA_StillUnableToObtainResources()
        {
            return new Exception("LiveCheckAvailable: Resources are still unavailable.");
        }

        public static Exception ObjectPolHasBeenReleased(string policyName)
        {
            return new ObjectDisposedException($"【{policyName}】 The {policyName} object pool has been released and cannot be accessed.");
        }

        public static Exception StatusIsNotAvailable(string policyName, string exceptionMessage)
        {
            return new Exception($"【{policyName}】 The status of {policyName} is unavailable. It can be used only after the background checker resumes. {exceptionMessage}");
        }

        public static Exception ResourceAcquisitionTimeout(double seconds)
        {
            throw new TimeoutException($"Method 'ObjectPool.Get' Get resource timeout ({seconds} seconds).");
        }
        
        public static Exception NoResourcesAvailableForAsynchronousCalls(int asyncGetCapacity)
        {
            throw new OutOfMemoryException($"Method 'ObjectPool.GetAsync' When calling resources, there are no available resources (and the queue is too long). Policy.AsyncGetCapacity = {asyncGetCapacity}");
        }
        
    }
}