using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.ML.TestFrameworkCommon.ProcessDump
{
    internal static class ProcessUtil
    {
        private static string FindIndexedProcessName(int pid)
        {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexdName = null;

            for (var index = 0; index < processesByName.Length; index++)
            {
                processIndexdName = index == 0 ? processName : processName + "#" + index;
                var processId = new PerformanceCounter("Process", "ID Process", processIndexdName);
                if ((int)processId.NextValue() == pid)
                {
                    return processIndexdName;
                }
            }

            return processIndexdName;
        }

        private static int FindPidFromIndexedProcessName(string indexedProcessName)
        {
            var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
            return (int)parentId.NextValue();
        }

        public static int ParentId(this Process process)
        {
            try
            {
                return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Fail to get parent process id of {process.ProcessName}," +
                    $"with exception {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// Return the list of processes which are direct children of the provided <paramref name="process"/> 
        /// instance.
        /// </summary>
        /// <remarks>
        /// This is a best effort API.  It can be thwarted by process instances starting / stopping during
        /// the building of this list.
        /// </remarks>
        internal static List<Process> GetProcessChildren(Process process) => GetProcessChildrenCore(process, Process.GetProcesses());

        private static List<Process> GetProcessChildrenCore(Process parentProcess, IEnumerable<Process> processes)
        {
            var list = new List<Process>();
            foreach (var process in processes)
            {
                var parentId = process.ParentId();

                Console.WriteLine($"{parentProcess.Id} : {parentId}");

                if (parentId == parentProcess.Id)
                {
                    list.Add(process);
                }
            }

            return list;
        }

        /// <summary>
        /// Return the list of processes which are direct or indirect children of the provided <paramref name="process"/> 
        /// instance.
        /// </summary>
        /// <remarks>
        /// This is a best effort API.  It can be thwarted by process instances starting / stopping during
        /// the building of this list.
        /// </remarks>
        internal static List<Process> GetProcessTree(Process process)
        {
            var processes = Process.GetProcesses();
            var list = new List<Process>();
            var toVisit = new Queue<Process>();
            toVisit.Enqueue(process);

            while (toVisit.Count > 0)
            {
                var cur = toVisit.Dequeue();
                var children = GetProcessChildrenCore(cur, processes);
                foreach (var child in children)
                {
                    toVisit.Enqueue(child);
                    list.Add(child);
                }
            }

            return list;
        }
    }
}
