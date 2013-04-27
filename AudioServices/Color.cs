using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicServices
{
    class Color
    {
        public static void WriteLineColor(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;

            Console.WriteLine(("[" + DateTime.Now.ToLongTimeString() + "] " + value).PadRight(Console.WindowWidth - 1));

            Console.ResetColor();
        }
    }
}
