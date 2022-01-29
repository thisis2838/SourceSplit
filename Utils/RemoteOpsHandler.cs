using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static LiveSplit.SourceSplit.WinAPI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LiveSplit.SourceSplit
{
    public class RemoteOpsHandler
    {

        public Process CurProcess;

        public RemoteOpsHandler(Process process)
        {
            CurProcess = process;
        }

        public uint CallFunctionString(string input, IntPtr funcPtr)
        {
            if (CurProcess == null || funcPtr == IntPtr.Zero || CurProcess.HasExited || CurProcess.Handle == IntPtr.Zero)
                return 0;

            IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                false,
                CurProcess.Id);

            uint bufSize = (uint)((input.Length + 1) * Marshal.SizeOf(typeof(char)));

            IntPtr stringBuf = VirtualAllocEx(
                procHandle,
                IntPtr.Zero,
                (UIntPtr)bufSize,
                (uint)(MemPageState.MEM_COMMIT | MemPageState.MEM_RESERVE),
                MemPageProtect.PAGE_READWRITE);

            if (stringBuf == IntPtr.Zero)
                return 0;

            WriteProcessMemory(procHandle, stringBuf, Encoding.Default.GetBytes(input), (UIntPtr)bufSize, out UIntPtr bytesWritten);
            var s = CreateRemoteThread(procHandle, IntPtr.Zero, UIntPtr.Zero, funcPtr, stringBuf, 0, out _);

            uint ret = 0;

            if (s != IntPtr.Zero)
            {
                WaitForSingleObject(s, 0xFFFFFFFF);
                GetExitCodeThread(s, out ret);
                TerminateThread(s, 0);
                CloseHandle(s);
            }

            VirtualFreeEx(procHandle, stringBuf, (UIntPtr)bufSize, (uint)MemPageState.MEM_RELEASE);

            return ret;
        }

        public uint CallFunction(uint input, IntPtr funcPtr)
        {
            if (CurProcess == null || funcPtr == IntPtr.Zero || CurProcess.HasExited || CurProcess.Handle == IntPtr.Zero)
                return 0;

            IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                false,
                CurProcess.Id);

            var s = CreateRemoteThread(procHandle, IntPtr.Zero, UIntPtr.Zero, funcPtr, (IntPtr)input, 0, out _);

            uint ret = 0;

            if (s != IntPtr.Zero)
            {
                WaitForSingleObject(s, 0xFFFFFFFF);
                GetExitCodeThread(s, out ret);
                TerminateThread(s, 0);
                CloseHandle(s);
            }
            return ret;
        }
    }
}
