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

                int filesCounter = 0;
                foreach (string file in Directory.GetFiles(pathEntered, "*.lua", SearchOption.AllDirectories))
                {
                    if (!Directory.Exists(@".\backups\"))
                    {
                        Directory.CreateDirectory(@".\backups\");
                    }

                    var currentFile = new FileInfo(file);

                    if (!File.Exists($@".\backups\{currentFile.Name}"))
                    {
                        File.Copy(file, $@".\backups\{currentFile.Name}");
                    }
                    else
                    {
                        File.Delete($@".\backups\{currentFile.Name}");
                        File.Copy(file, $@".\backups\{currentFile.Name}");
                    }

                    Console.WriteLine($"File: {currentFile.Name}");
                    Stopwatch watchFile = new Stopwatch();
                    watchFile.Start();
                    string[] lines = File.ReadAllLines(file);
                    if (lines.Length > 0)
                    {
                        using (StreamWriter f = new StreamWriter($@"{file}"))
                        {
                            for (int currLine = 0; currLine < lines.Length; currLine++)
                            {
                                if (!lines[currLine].StartsWith("--"))
                                {
                                    if (lines[currLine].Contains("AddEventHandler(") || lines[currLine].Contains("RegisterNetEvent(") || lines[currLine].Contains("TriggerServerEvent(") || lines[currLine].Contains("TriggerClientEvent(") || lines[currLine].Contains("TriggerEvent(") || lines[currLine].Contains("RegisterServerEvent("))
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
                                        // Erreur causé : si les deux, aucun n'est trouvé

                                        try
                                        {
                                            string nameOfEvent = linesSp[1];
                                            lines[currLine] = lines[currLine].Replace(nameOfEvent, Base64Encode(nameOfEvent));
                                        }
                                        catch (IndexOutOfRangeException ex)
                                        {
                                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                                            Console.WriteLine($"{currLine}: skipped event. ({lines[currLine]})");
                                            Console.ResetColor();
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex);
                                            return;
                                        }
                                    }
                                }
                            }

                            filesCounter++;
                        }

                        File.WriteAllLines(file, lines);
                        watchFile.Stop();
                        if (watchFile.Elapsed.TotalMilliseconds > 1000)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Replaced for {currentFile.Name}\nProcess (replaced {lines.Length} lines) took: {watchFile.Elapsed.TotalSeconds} seconds");
                            Console.ResetColor();
                        }
                        else if (watchFile.Elapsed.TotalMilliseconds > 5000)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Replaced for {currentFile.Name}\nProcess (replaced {lines.Length} lines) took: {watchFile.Elapsed.TotalSeconds} seconds");
                            Console.ResetColor();
                        }
                        else if (watchFile.Elapsed.TotalMilliseconds < 200)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Replaced for {currentFile.Name}\nProcess (replaced {lines.Length} lines) took: {watchFile.Elapsed.TotalSeconds} seconds");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ResetColor();
                            Console.WriteLine($"Replaced for {currentFile.Name}\nProcess (replaced {lines.Length} lines) took: {watchFile.Elapsed.TotalSeconds} seconds");
                        }
                    }
                }
                watchFolder.Stop();
                Console.WriteLine($"Process (all files: {filesCounter}) took: {watchFolder.Elapsed.TotalSeconds} seconds");
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

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}