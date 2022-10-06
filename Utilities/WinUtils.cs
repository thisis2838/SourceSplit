using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utilities
{
    using SizeT = UIntPtr;

    public static class WinUtils
    {
        #region SEND MESSAGE
        public const int WM_COPYDATA = 0x4a;

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }
        [DllImport("User32")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, ref COPYDATASTRUCT lParam);
        public static void SendMessage(Process proc, string input)
        {
            if (proc == null || proc.HasExited || proc.Handle == IntPtr.Zero || input.Length == 0)
                return;

            input = input + "\0";

            var copy = new COPYDATASTRUCT()
            {
                cbData = input.Length,
                dwData = IntPtr.Zero,
                lpData = Marshal.StringToHGlobalAnsi(input)
            };
            int res = SendMessage(proc.MainWindowHandle, WM_COPYDATA, 0, ref copy);
        }
        #endregion

        #region CALL FUNCTION
        public enum MemPageState : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_RESERVE = 0x2000,
            MEM_FREE = 0x10000,
            MEM_RELEASE = 0x8000
        }

        [Flags]
        public enum MemPageProtect : uint
        {
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400,
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer,
            SizeT nSize, out SizeT lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, SizeT dwSize, uint flAllocationType,
            MemPageProtect flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, SizeT dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, SizeT dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll")]
        public static extern uint WaitForSingleObject(IntPtr handle, uint time);

        [DllImport("kernel32.dll")]
        public static extern uint GetExitCodeThread([In] IntPtr handle, out uint lpExitCode);

        [DllImport("kernel32.dll")]
        public static extern bool TerminateThread(IntPtr handle, uint code);

        public const int PROCESS_CREATE_THREAD = 0x0002;
        public const int PROCESS_QUERY_INFORMATION = 0x0400;
        public const int PROCESS_VM_OPERATION = 0x0008;
        public const int PROCESS_VM_WRITE = 0x0020;
        public const int PROCESS_VM_READ = 0x0010;

        #endregion


        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("winmm.dll")]
        public static extern uint timeBeginPeriod(uint uMilliseconds);
        [DllImport("winmm.dll")]
        public static extern uint timeEndPeriod(uint uMilliseconds);
    }
}
