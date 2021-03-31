using System;
using System.IO;

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
                OnBadArgument("Please, choose a good key! Press a key to reset.");
                return;
            }

            Console.WriteLine("Specify a path");
            string pathEntered = Console.ReadLine();

            if (!Directory.Exists(pathEntered))
            {
                OnBadArgument("Please, choose a working path! Press a key to reset.");
                return;
            }
        }

        private static void OnBadArgument(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            Console.ReadKey();
            Main();
        }
    }
}