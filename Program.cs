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
            Console.ResetColor();

            Console.WriteLine("Welcome!\n\n");

            Console.WriteLine("Specify a path");
            Console.ForegroundColor = ConsoleColor.Cyan;
            string pathEntered = Console.ReadLine();
            Console.ResetColor();

            if (!Directory.Exists(pathEntered))
            {
                OnBadArgument("Please, choose a working path! Press a key to retry.");
                return;
            }

            bool logs = false;
            Console.WriteLine("Enable logs: y/n");
            string enableLogs = Console.ReadLine();

            if (string.IsNullOrEmpty(enableLogs) || enableLogs != "y" && enableLogs != "n")
            {
                OnBadArgument("Please, choose yes (y) or no (n)! Press a key to retry.");
                return;
            }
            else
            {
                if (enableLogs == "y")
                {
                    logs = true;
                }
            }

            Console.Clear();
            try
            {
                Stopwatch watchFolder = new Stopwatch();
                watchFolder.Start();

                int filesCounter = 0;
                int linesCounter = 0;
                int totalLinesSkipped = 0;
                int totalLinesReplaced = 0;

                bool directoryExist = true;
                int iDir = 1;
                while (directoryExist)
                {
                    if (!Directory.Exists($@".\backups-{iDir}\"))
                    {
                        Directory.CreateDirectory($@".\backups-{iDir}\");
                        directoryExist = false;
                        break;
                    }
                    iDir++;
                }

                if (logs)
                {
                    Stopwatch watchSave = new Stopwatch();
                    watchSave.Start();
                    var dirInfos = new DirectoryInfo(pathEntered);
                    Console.WriteLine($"Saving {dirInfos.Name} to backups-{iDir}...");
                    DirectoryCopy(pathEntered, $@".\backups-{iDir}\", true);
                    watchSave.Stop();
                    Console.WriteLine($"Saved to backups-{iDir}: took {Math.Round(watchSave.Elapsed.TotalSeconds, 2)} seconds");
                }
                else
                {
                    DirectoryCopy(pathEntered, $@".\backups-{iDir}\", true);
                }

                string[] path = Directory.GetFiles(pathEntered, "*.lua", SearchOption.AllDirectories);

                foreach (string file in path)
                {
                    if (!logs)
                    {
                        double percentage = (double)filesCounter / (double)path.Length * 100;
                        Console.WriteLine($"Processing: {percentage}%");
                    }

                    var currentFile = new FileInfo(file);

                    Stopwatch watchFile = new Stopwatch();
                    if (logs)
                    {
                        Console.WriteLine($"File: {currentFile.Name}");
                        watchFile.Start();
                    }

                    string[] lines = File.ReadAllLines(file);
                    if (lines.Length > 0)
                    {
                        linesCounter += lines.Length;
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
                                        else
                                        {
                                            if (FindFirstCharContainsString(lines[currLine]) == "\"")
                                            {
                                                linesSp = lines[currLine].Split('"');
                                            }
                                            else if (FindFirstCharContainsString(lines[currLine]) == "\'")
                                            {
                                                linesSp = lines[currLine].Split('\'');
                                            }
                                            else if (FindFirstCharContainsString(lines[currLine]) == "unfinded" && logs)
                                            {
                                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                                Console.WriteLine($"{currLine + 1}: skipped event. ({lines[currLine]})");
                                                Console.ResetColor();
                                            }
                                        }

                                        try
                                        {
                                            string nameOfEvent = linesSp[1];
                                            if (!IsFivemEvent(nameOfEvent))
                                            {
                                                lines[currLine] = lines[currLine].Replace(nameOfEvent, Base64Encode(nameOfEvent));
                                                totalLinesReplaced++;
                                            }
                                            else
                                            {
                                                totalLinesSkipped++;
                                                if (logs)
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                                    Console.WriteLine($"{currLine + 1}: skipped event (FiveM event).");
                                                    Console.ResetColor();
                                                }
                                            }
                                        }
                                        catch (IndexOutOfRangeException)
                                        {
                                            totalLinesSkipped++;
                                            if (logs)
                                            {
                                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                                Console.WriteLine($"{currLine + 1}: skipped event (out of range). ({lines[currLine]})");
                                                Console.ResetColor();
                                            }
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
                        if (logs)
                        {
                            watchFile.Stop();
                            if (watchFile.Elapsed.TotalMilliseconds > 1000)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Replaced for {currentFile.Name}\nProcess (replaced {lines.Length} lines) took: {Math.Round(watchFile.Elapsed.TotalSeconds, 5)} seconds");
                                Console.ResetColor();
                            }
                            else if (watchFile.Elapsed.TotalMilliseconds > 5000)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Replaced for {currentFile.Name}\nProcess (replaced {lines.Length} lines) took: {Math.Round(watchFile.Elapsed.TotalSeconds, 5)} seconds");
                                Console.ResetColor();
                            }
                            else if (watchFile.Elapsed.TotalMilliseconds < 200)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Replaced for {currentFile.Name}\nProcess (replaced {lines.Length} lines) took: {Math.Round(watchFile.Elapsed.TotalSeconds, 5)} seconds");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.ResetColor();
                                Console.WriteLine($"Replaced for {currentFile.Name}\nProcess (replaced {lines.Length} lines) took: {Math.Round(watchFile.Elapsed.TotalSeconds, 5)} seconds");
                            }
                        }
                        else if (!logs)
                        {
                            Console.Clear();
                        }
                    }
                }
                watchFolder.Stop();
                Console.WriteLine($"--------------------------------------------------------------");
                Console.WriteLine($"Process took: {Math.Round(watchFolder.Elapsed.TotalSeconds, 2)} seconds");
                Console.WriteLine($"Total files: {filesCounter}");
                Console.WriteLine($"Total lines: {linesCounter}");
                Console.WriteLine($"Total lines skipped: {totalLinesSkipped}");
                Console.WriteLine($"Total lines replaced: {totalLinesReplaced}");
                Console.WriteLine($"--------------------------------------------------------------");
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

        public static string FindFirstCharContainsString(string line)
        {
            string def = "unfinded";
            foreach (char c in line)
            {
                if (c == '"')
                {
                    return c.ToString();
                }
                else if (c == '\'')
                {
                    return c.ToString();
                }
            }
            return def;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".lua")
                {
                    string tempPath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(tempPath, false);
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        private static bool IsFivemEvent(string eventName)
        {
            string[] fivemEvents =
            {
                "gameEventTriggered",
                "onResourceStart",
                "populationPedCreating",
                "onClientResourceStart",
                "onResourceStop",
                "onClientResourceStop",
                "playerSpawned",
                "onClientMapStart",
                "onClientGameTypeStart",
                "onClientMapStop",
                "onClientGameTypeStop",
                "getMapDirectives",
                "onPlayerDied",
                "onPlayerKilled",
                "playerActivated",
                "sessionInitialized",
                "chatMessage",
                "chat:addMessage",
                "chat:addTemplate",
                "chat:removeSuggestion",
                "chat:clear",
                "entityCreated",
                "playerEnteredScope",
                "entityCreating",
                "entityRemoved",
                "onResourceListRefresh",
                "onServerResourceStart",
                "onServerResourceStop",
                "playerConnecting",
                "playerJoining",
                "playerLeftScope",
                "startProjectileEvent",
                "onResourceStarting",
                "playerDropped",
                "rconCommand",
                "weaponDamageEvent",
                "vehicleComponentControlEvent",
                "respawnPlayerPedEvent",
                "explosionEvent"
            };

            foreach (string fivemEventName in fivemEvents)
            {
                if (eventName == fivemEventName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}