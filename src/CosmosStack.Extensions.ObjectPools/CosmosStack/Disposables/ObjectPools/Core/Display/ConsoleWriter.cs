using System;

namespace CosmosStack.Disposables.ObjectPools.Core.Display
{
    /// <summary>
    /// Display helper
    /// </summary>
    internal static class ConsoleWriter
    {
        public static void Unavailable(string message)
        {
            using (ConsoleColorWorker.Start(ConsoleColorSuit.UnavailableSuit))
            {
                Console.Write(message);
            }

            Console.WriteLine();
        }

        public static void Available(string message)
        {
            using (ConsoleColorWorker.Start(ConsoleColorSuit.AvailableSuit))
            {
                Console.Write(message);
            }

            Console.WriteLine();
        }
    }
}