using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static LiveSplit.SourceSplit.Utilities.WinUtils;
using MemPageProtect = LiveSplit.SourceSplit.Utilities.WinUtils.MemPageProtect;
using MemPageState = LiveSplit.SourceSplit.Utilities.WinUtils.MemPageState;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class ProcUtils
    {
        public static ProcessModuleWow64Safe[] ModulesWow64SafeNoCache(this Process p)
        {
            const int LIST_MODULES_ALL = 3;
            const int MAX_PATH = 260;

            var hModules = new IntPtr[1024];

            uint cb = (uint)IntPtr.Size * (uint)hModules.Length;
            uint cbNeeded;

            if (!ComponentUtil.WinAPI.EnumProcessModulesEx(p.Handle, hModules, cb, out cbNeeded, LIST_MODULES_ALL))
                throw new Win32Exception();
            uint numMods = cbNeeded / (uint)IntPtr.Size;

            var ret = new List<ProcessModuleWow64Safe>();

            // everything below is fairly expensive, which is why we cache!
            var sb = new StringBuilder(MAX_PATH);
            for (int i = 0; i < numMods; i++)
            {
                sb.Clear();
                if (WinUtils.GetModuleFileNameEx(p.Handle, hModules[i], sb, sb.Capacity) == 0)
                    throw new Win32Exception();
                string fileName = sb.ToString();

                sb.Clear();
                if (WinUtils.GetModuleBaseName(p.Handle, hModules[i], sb, (uint)sb.Capacity) == 0)
                    throw new Win32Exception();
                string baseName = sb.ToString();

                var moduleInfo = new WinAPI.MODULEINFO();
                if (!WinAPI.GetModuleInformation(p.Handle, hModules[i], out moduleInfo, (uint)Marshal.SizeOf(moduleInfo)))
                    throw new Win32Exception();

                ret.Add(new ProcessModuleWow64Safe()
                {
                    FileName = fileName,
                    BaseAddress = moduleInfo.lpBaseOfDll,
                    ModuleMemorySize = (int)moduleInfo.SizeOfImage,
                    EntryPointAddress = moduleInfo.EntryPoint,
                    ModuleName = baseName
                });
            }

            return ret.ToArray();
        }

        public static int SendMessage(this Process proc, string input)
        {
            if (proc == null || proc.HasExited || proc.Handle == IntPtr.Zero || input.Length == 0)
                return 0;

            input = input + "\0";

            var copy = new COPYDATASTRUCT()
            {
                cbData = input.Length,
                dwData = IntPtr.Zero,
                lpData = Marshal.StringToHGlobalAnsi(input)
            };

            int res = WinUtils.SendMessage(proc.MainWindowHandle, WM_COPYDATA, 0, ref copy);
            return res;
        }

        public static uint CallFunctionString(this Process process, string input, IntPtr funcPtr)
        {
            if (process == null || funcPtr == IntPtr.Zero || process.HasExited || process.Handle == IntPtr.Zero)
                return 0;

            IntPtr procHandle = OpenProcess
            (
                PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | 
                PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                false,
                process.Id
            );

            uint bufSize = (uint)((input.Length + 1) * Marshal.SizeOf(typeof(char)));

            IntPtr stringBuf = VirtualAllocEx
            (
                procHandle,
                IntPtr.Zero,
                (UIntPtr)bufSize,
                (uint)(MemPageState.MEM_COMMIT | MemPageState.MEM_RESERVE),
                MemPageProtect.PAGE_READWRITE
            );

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

    }
}
