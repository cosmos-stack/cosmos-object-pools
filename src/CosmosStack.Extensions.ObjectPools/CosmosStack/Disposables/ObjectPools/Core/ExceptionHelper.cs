using System;
using CosmosStack.Disposables.ObjectPools.Statistics;

namespace CosmosStack.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Exception helper
    /// </summary>
    internal static class ExceptionHelper
    {
        public static Exception CA_UnableToObtainResources(StatisticsInfo statistics)
        {
            return new($"CheckAvailable: unable to obtain resource. {statistics}");
        }

        public static Exception CA_StillUnableToObtainResources()
        {
            return new("CheckAvailable: Resources are still unavailable.");
        }

        public static Exception LCA_UnableToObtainResources(StatisticsInfo statistics)
        {
            return new($"LiveCheckAvailable: unable to obtain resource. {statistics}");
        }

        public static Exception LCA_StillUnableToObtainResources()
        {
            return new("LiveCheckAvailable: Resources are still unavailable.");
        }

        public static Exception ObjectPolHasBeenReleased(string policyName)
        {
            return new ObjectDisposedException($"【{policyName}】 The {policyName} object pool has been released and cannot be accessed.");
        }

        public static Exception StatusIsNotAvailable(string policyName, string exceptionMessage)
        {
            return new($"【{policyName}】 The status of {policyName} is unavailable. It can be used only after the background checker resumes. {exceptionMessage}");
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