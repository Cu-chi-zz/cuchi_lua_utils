using System;

namespace cuchi_lua_utils
{
    internal class Program
    {
        private static void Main()
        {
            Console.Clear();
            Console.WriteLine("Choose a key to modify: events/vars");
            string keyChoosed = Console.ReadLine();

            if (string.IsNullOrEmpty(keyChoosed) || keyChoosed != "events" && keyChoosed != "vars")
            {
                Console.WriteLine("Please, choose a good key! Press a key to reset.");
                Console.ReadKey();
                Main();
                return;
            }
        }
    }
}