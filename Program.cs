using System;
using System.Diagnostics;
using System.IO;
using System.Text;

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

            try
            {
                Stopwatch watchFolder = new Stopwatch();
                watchFolder.Start();
                foreach (string file in Directory.GetFiles(pathEntered, "*.lua", SearchOption.AllDirectories))
                {
                    Stopwatch watchFile = new Stopwatch();
                    watchFile.Start();
                    string[] lines = File.ReadAllLines(file);
                    using (StreamWriter f = new StreamWriter($@"{file}"))
                    {
                        for (int currLine = 0; currLine < lines.Length; currLine++)
                        {
                            if (lines[currLine].StartsWith("EventHandler"))
                            {
                                string[] linesSp = { };
                                if (lines[currLine].Contains('"') && !lines[currLine].Contains('\''))
                                {
                                    linesSp = lines[currLine].Split('"');
                                }
                                else if (lines[currLine].Contains('\'') && !lines[currLine].Contains('"'))
                                {
                                    linesSp = lines[currLine].Split('\'');
                                }

                                string nameOfEvent = linesSp[1];
                                lines[currLine] = lines[currLine].Replace(nameOfEvent, Base64Encode(nameOfEvent));
                            }
                        }
                    }

                    File.WriteAllLines(file, lines);
                    watchFile.Stop();
                    Console.WriteLine($"Process (replace lines) took: {watchFile.Elapsed.TotalSeconds} seconds");
                }
                watchFolder.Stop();
                Console.WriteLine($"Process (all folders) took: {watchFolder.Elapsed.TotalSeconds} seconds");
            }
            catch (UnauthorizedAccessException ex)
            {
                OnBadArgument($"Error, can't access to {pathEntered}.\n{ex}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
        }

        private static void OnBadArgument(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            Console.ReadKey();
            Main();
        }

        private static string GenerateString(int length)
        {
            StringBuilder str_build = new StringBuilder();
            Random random = new Random();

            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                str_build.Append(letter);
            }
            return str_build.ToString();
        }

        private static int GenerateInt(int lengthMin, int lengthMax)
        {
            Random random = new Random();
            return random.Next(lengthMin, lengthMax);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}