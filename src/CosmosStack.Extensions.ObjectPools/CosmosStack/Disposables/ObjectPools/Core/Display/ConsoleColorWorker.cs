using System;

namespace CosmosStack.Disposables.ObjectPools.Core.Display
{
    internal class ConsoleColorWorker : IDisposable
    {
        private ConsoleColorWorker(ConsoleColorSuit options)
        {
            BackgroundColor = Console.BackgroundColor;
            ForegroundColor = Console.ForegroundColor;

            Console.BackgroundColor = options.BackgroundColor;
            Console.ForegroundColor = options.ForegroundColor;
        }

        private ConsoleColor BackgroundColor { get; set; }

        private ConsoleColor ForegroundColor { get; set; }

        public void Dispose()
        {
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;
        }

        public static ConsoleColorWorker Start(ConsoleColorSuit options) => new(options);
    }
}