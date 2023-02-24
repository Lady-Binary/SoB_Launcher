using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;
using System.Reflection;

namespace SoB_Launcher
{
    class Program
    {
        static int VERBOSITY = 0;
        const int VERBOSITY_DEBUG = 1;

        //
        // This method logs a Debug string to console
        //
        static void Debug(string s = "")
        {
            if (VERBOSITY >= VERBOSITY_DEBUG)
                Console.WriteLine(s);
        }

        //
        // This method logs a string to console
        //
        static void Log(string s = "")
        {
            Console.WriteLine(s);
        }

        //
        // This method parses the command line parameters
        //
        static void ParseCommandLineParams(string[] args)
        {
            foreach (var arg in args)
            {
                switch (arg)
                {
                    case "-v":
                    case "--verbose":
                        {
                            VERBOSITY++;
                            break;
                        }

                    default:
                        {
                            Log($"Invalid command line param: {arg}");
                            WaitForEnterKeyAndExit(-1);
                            break;
                        }
                }
                Console.WriteLine(arg);
            }
        }

        //
        // This method pauses the execution and waits for the user to press the Enter key
        //
        static void WaitForEnterKey()
        {
            Log();
            Log("Press the Enter key to continue.");
            Log();
            Console.ReadLine();
        }

        //
        // This method pauses the execution, waits for the user to press the Enter key,
        // then exits the program with the given exit code.
        //
        static void WaitForEnterKeyAndExit(int exitCode)
        {
            Log();
            Log("Press the Enter key to exit.");
            Log();
            Console.ReadLine();
            Environment.Exit(exitCode);
        }

        //
        // This method ensures we are the only process named "SoB_Launcher".
        // If we are not alone, it exits the program.
        //
        static void EnsureWeAreTheOnlySoBLauncher()
        {
            Debug("Checking for other SoB_Launcher processes...");
            if (Process.GetProcessesByName("SoB_Launcher").Length > 1)
            {
                Log("More than one process named SoB_Launcher was found: cannot continue!");
                Log();
                Log("Make sure all SoB Launchers are closed.");
                Log("Also check your system tray, a SoB Launcher might still be running.");
                Log("If you really can't find any launcher open, then restart your computer.");
                Log();
                Log("PLEASE CLOSE ALL SOB LAUNCHERS YOU MAY HAVE STARTED, AND THEN RE-RUN THIS LAUNCHER.");
                Log();
                WaitForEnterKeyAndExit(-1);
            }
            Debug($"...no other SoB Launcher processes found, all good, continuing.");
            Debug();
        }

        //
        // This function searches for the Legends of Aria.exe executable within the current folder.
        //
        static string SearchAriaExecutable()
        {
            Debug("Searching for Legends of Aria.exe within the current directory...");
            var currentFullPath = Process.GetCurrentProcess().MainModule?.FileName;
            var currentDirectory = Path.GetDirectoryName(currentFullPath);
            currentDirectory = @"E:\Legends of Aria-1.4.1.0\Legends of Aria";
            Debug($"...current directory: {currentDirectory}...");
            var ariaFullPath = $"{currentDirectory}\\Legends of Aria.exe";
            if (!File.Exists(ariaFullPath))
            {

                Log($"Could not find \"Legends of Aria.exe\" in the current folder {currentDirectory}!");
                Log();
                Log("(Better\u2122) SoB Launcher must be run from the same directory where \"Legends of Aria.exe\" resides.");
                Log("This could be for instance \"C:\\Games\\Shards of Britannia\\\".");
                Log();
                Log("If the official \"SoB_Launcher.exe\" executable is also present in that folder, then rename it to");
                Log("something like \"SoB_Launcher_Original.exe\", but DO NOT OVERWRITE IT OR DELETE IT: you may need it in the future.");
                Log();
                Log("PLEASE READ THE INSTRUCTIONS ABOVE, MOVE BETTER SOB LAUNCHER INTO THE CORRECT FOLDER AND RE-LAUNCH IT.");
                Log();
                WaitForEnterKeyAndExit(-1);
            }
            Debug($"...Aria Client found at {ariaFullPath}, all good, continuing!");
            Debug();
            return ariaFullPath;
        }

        //
        // This function starts the Legends of Aria.exe executable and returns its PID.
        //
        static int StartAriaProcess(string ariaFullPath)
        {
            bool started = false;
            var p = new Process();

            p.StartInfo.FileName = ariaFullPath;

            Log("Starting the Aria Client...");
            started = p.Start();

            Debug("...sleeping for 5 seconds, waiting for the Aria Client to start...");
            Thread.Sleep(5000);

            var ariaPID = -1;
            try
            {
                ariaPID = p.Id;
            }
            catch (InvalidOperationException)
            {
                started = false;
            }
            catch (Exception)
            {
                started = false;
            }

            if (!started || ariaPID < 0)

            {
                Log("Could not start the Legends of Aria process!");
                Log();
                Log("Something is wrong with your client.");
                Log("Please ensure you can start it manually.");
                Log();
                Log("PLEASE CHECK ANY ERROR MESSAGE THAT MAY HAVE APPEARED, AND RE-RUN THIS LAUNCHER.");
                Log();
                WaitForEnterKeyAndExit(-1);
            }
            Debug($"...Legends of Aria client started successfully with PID {ariaPID}!");
            Debug();

            return ariaPID;
        }

        //
        // This method writes the given PID number to a sob.pid file within the same
        // folder where Legends of Aria.exe resides.
        //
        static string WritePidFile(string ariaFullPath, int ariaPID)
        {
            var ariaDirectory = Path.GetDirectoryName(ariaFullPath);
            var pidFullPath = $"{ariaDirectory}\\sob.pid";
            Debug($"Writing PID {ariaPID} to {pidFullPath}...");
            File.WriteAllText(pidFullPath, ariaPID.ToString());
            Debug($"...PID {ariaPID} written to {pidFullPath} successfully!");
            return pidFullPath;
        }

        //
        // This method pauses execution until the Aria Client is closed
        //
        static void SleepUntilAriaClientIsClosed(int ariaPID)
        {
            while (Process.GetProcesses().Any(x => x.Id == ariaPID))
            {
                //Client still running
                Thread.Sleep(1000);
            }
            Debug("The Aria Client was closed.");
            Debug();
        }

        //
        // The Main method
        //
        static void Main(string[] args)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Log();
            Log($"(Better\u2122) SoB Launcher v{version} - made by Lady Binary with \u2665");
            Log();

            ParseCommandLineParams(args);

            EnsureWeAreTheOnlySoBLauncher();

            var ariaFullPath = SearchAriaExecutable();

            var ariaPID = StartAriaProcess(ariaFullPath);

            var pidFullPath = WritePidFile(ariaFullPath, ariaPID);

            Log();
            Log("All done! The Aria Client is now loading.");
            Log();
            Log("As soon as the Aria Client is ready you can safely inject EasyLoU as usual if you wish to do so.");
            Log();
            Log("LEAVE THIS WINDOW OPEN, or the game client will exit.");
            Log();
            Log("Have fun :)");
            Log();

            SleepUntilAriaClientIsClosed(ariaPID);

            Log("The Aria Client was closed: you can safely close this window now.");
            WaitForEnterKeyAndExit(0);
        }
    }
}
