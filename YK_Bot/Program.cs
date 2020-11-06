using System;

namespace YK_Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            new Bot().RunBot().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
