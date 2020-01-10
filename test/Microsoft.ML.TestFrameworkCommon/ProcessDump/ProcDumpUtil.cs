using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Microsoft.ML.TestFrameworkCommon.ProcessDump
{
    internal static class ProcDumpUtil
    {
        internal readonly struct ProcDumpInfo
        {
            internal string ProcDumpFilePath { get; }
            internal string DumpDirectory { get; }

            internal ProcDumpInfo(string procDumpFilePath, string dumpDirectory)
            {
                ProcDumpFilePath = procDumpFilePath;
                DumpDirectory = dumpDirectory;
            }
        }
        internal static Process AttachProcDump(ProcDumpInfo procDumpInfo, int processId)
        {
            return AttachProcDump(procDumpInfo.ProcDumpFilePath, processId, procDumpInfo.DumpDirectory);
        }

        internal static string GetProcDumpCommandLine(int processId, string dumpDirectory)
        {
            // /accepteula command line option to automatically accept the Sysinternals license agreement.
            // -ma	Write a 'Full' dump file. Includes All the Image, Mapped and Private memory.
            // -e	Write a dump when the process encounters an unhandled exception. Include the 1 to create dump on first chance exceptions.
            // -f C00000FD.STACK_OVERFLOWC Dump when a stack overflow first chance exception is encountered. 
            const string procDumpSwitches = "/accepteula -ma -e -f C00000FD.STACK_OVERFLOW";
            dumpDirectory = dumpDirectory.TrimEnd('\\');
            return $" {procDumpSwitches} {processId} \"{dumpDirectory}\"";
        }

        /// <summary>
        /// Attaches a new procdump.exe against the specified process.
        /// </summary>
        /// <param name="procDumpFilePath">The path to the procdump executable</param>
        /// <param name="processId">process id</param>
        /// <param name="dumpDirectory">destination directory for dumps</param>
        internal static Process AttachProcDump(string procDumpFilePath, int processId, string dumpDirectory)
        {
            Directory.CreateDirectory(dumpDirectory);
            return Process.Start(procDumpFilePath, GetProcDumpCommandLine(processId, dumpDirectory));
        }
    }
}
