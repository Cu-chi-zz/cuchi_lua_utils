using System;

namespace cuchi_lua_utils
{
    internal class Program
    {
        private static void Main()
        {
            Console.Clear();
            Console.WriteLine("Choose a key to modify:");
            string keyChoosed = Console.ReadLine();

            if (string.IsNullOrEmpty(keyChoosed))
            {
                Console.WriteLine("Please, choose a key!");
                Console.ReadKey();
                Main();
                return;
            }
        }
    }
}