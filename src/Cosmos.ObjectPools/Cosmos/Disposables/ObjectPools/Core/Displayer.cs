using System;

namespace Cosmos.Disposables.ObjectPools.Core
{
    /// <summary>
    /// Display helper
    /// </summary>
    internal static class Displayer
    {
        public static void Unavailable(string message)
        {
            var bgColor = Console.BackgroundColor;
            var foreColor = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(message);
            Console.BackgroundColor = bgColor;
            Console.ForegroundColor = foreColor;
            Console.WriteLine();
        }

        public static void Available(string message)
        {
            var bgColor = Console.BackgroundColor;
            var foreColor = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(message);
            Console.BackgroundColor = bgColor;
            Console.ForegroundColor = foreColor;
            Console.WriteLine();
        }
    }
}