using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ML.TestFrameworkCommon.ProcessDump.ProcDumpUtil;

namespace Microsoft.ML.TestFrameworkCommon.ProcessDump
{
    public class ProcDumpHelper
    {
        public static void TakeProcessDump()
        {
            string os = Environment.OSVersion.ToString();

            if(!os.Contains("Windows"))
            {
                Console.WriteLine("Only support take dumping of processes in windows.");
                return;
            }


            Console.WriteLine("Dumping remaining processes");
            Console.WriteLine($"Current directory is {Directory.GetCurrentDirectory()}");
            var procDumpInfo = GetProcDumpInfo();
            if (procDumpInfo != null)
            {
                //Take dump of current process now
                //var counter = 0;
                //foreach (var proc in ProcessUtil.GetProcessTree(Process.GetCurrentProcess()).OrderBy(x => x.ProcessName))
                //{
                //    var dumpDir = procDumpInfo.Value.DumpDirectory;

                //    var dumpFilePath = Path.Combine(dumpDir, $"{proc.ProcessName}-{counter}.dmp");
                //    DumpProcess(proc, procDumpInfo.Value.ProcDumpFilePath, dumpFilePath);
                //    counter++;
                //}

                //take dump of current process
                var currentProcess = Process.GetCurrentProcess();
                var dumpFilePath = $"{currentProcess.ProcessName}.dmp";
                DumpProcess(currentProcess, procDumpInfo.Value.ProcDumpFilePath, dumpFilePath);
            }
            else
            {
                Console.WriteLine("Could not locate procdump");
            }
        }

        private static ProcDumpInfo? GetProcDumpInfo()
        {
            string procDumpDirectory = "..\\..\\..\\..\\Tools\\";
            string dumpOutputDirectory = "\\";

            if (!string.IsNullOrEmpty(procDumpDirectory))
            {
                return new ProcDumpInfo(procDumpDirectory + "procdump.exe", dumpOutputDirectory);
            }

            return null;
        }

        private static void DumpProcess(Process targetProcess, string procDumpExeFilePath, string dumpFilePath)
        {
            var name = targetProcess.ProcessName;

            // Our space for saving dump files is limited. Skip dumping for processes that won't contribute
            // to bug investigations.
            if (name == "procdump" || name == "conhost")
            {
                return;
            }

            if(!File.Exists(procDumpExeFilePath))
            {
                if(Directory.Exists("..\\..\\..\\..\\Tools\\"))
                {
                    Console.WriteLine("..\\..\\..\\..\\Tools\\ directory exists but procdump.exe not exist.");
                    string[] fileEntries = Directory.GetFiles("..\\..\\..\\..\\Tools\\");
                    foreach (string fileName in fileEntries)
                        Console.WriteLine($"     --debug: {fileName}");
                }
                Console.WriteLine("..\\..\\..\\..\\Tools\\ directory not exists.");
            }
            else
            {
                Console.WriteLine($"{procDumpExeFilePath} exists.");
            }

            Console.WriteLine($"procDumpExeFilePath {procDumpExeFilePath}.");
            Console.WriteLine($"Dumping {name} {targetProcess.Id} to {dumpFilePath} ... ");
            try
            {
                var args = $"-accepteula -ma {targetProcess.Id} {dumpFilePath}";
                var processInfo = ProcessRunner.CreateProcess(procDumpExeFilePath, args);
                var processOutput = processInfo.Result.Result;

                // The exit code for procdump doesn't obey standard windows rules.  It will return non-zero
                // for successful cases (possibly returning the count of dumps that were written).  Best 
                // backup is to test for the dump file being present.
                if (File.Exists(dumpFilePath))
                {
                    Console.WriteLine($"succeeded ({new FileInfo(dumpFilePath).Length} bytes)");
                }
                else
                {
                    Console.WriteLine($"FAILED with {processOutput.ExitCode}");
                    Console.WriteLine(string.Join(Environment.NewLine, processOutput.ErrorLines));
                    Console.WriteLine($"{procDumpExeFilePath} {args}");
                    Console.WriteLine(string.Join(Environment.NewLine, processOutput.OutputLines));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED to take dump with exception: {ex.Message}");
            }
        }
    }
}
