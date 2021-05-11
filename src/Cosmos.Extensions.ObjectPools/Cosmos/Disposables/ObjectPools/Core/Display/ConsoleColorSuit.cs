using System;

namespace Cosmos.Disposables.ObjectPools.Core.Display
{
    internal class ConsoleColorSuit
    {
        public ConsoleColor BackgroundColor { get; internal set; }

        public ConsoleColor ForegroundColor { get; internal set; }

        public static ConsoleColorSuit AvailableSuit { get; }
            = new() {BackgroundColor = ConsoleColor.DarkGreen, ForegroundColor = ConsoleColor.White};

        public static ConsoleColorSuit UnavailableSuit { get; }
            = new() {BackgroundColor = ConsoleColor.DarkYellow, ForegroundColor = ConsoleColor.White};
    }
}