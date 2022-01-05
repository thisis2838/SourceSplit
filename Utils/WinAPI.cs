using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit
{
    using SizeT = UIntPtr;

    public enum MemPageState : uint
    {
        MEM_COMMIT = 0x1000,
        MEM_RESERVE = 0x2000,
        MEM_FREE = 0x10000,
        MEM_RELEASE = 0x8000
    }

    public enum MemPageType : uint
    {
        MEM_PRIVATE = 0x20000,
        MEM_MAPPED = 0x40000,
        MEM_IMAGE = 0x1000000
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

    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryBasicInformation // MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public MemPageProtect AllocationProtect;
        public SizeT RegionSize;
        public MemPageState State;
        public MemPageProtect Protect;
        public MemPageType Type;
    }

    public static class WinAPI
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer,
            SizeT nSize, out SizeT lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer,
            SizeT nSize, out SizeT lpNumberOfBytesWritten);

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumProcessModulesEx(IntPtr hProcess, [Out] IntPtr[] lphModule, uint cb,
            out uint lpcbNeeded, uint dwFilterFlag);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName,
            uint nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, [Out] out MODULEINFO lpmodinfo,
            uint cb);

        [DllImport("psapi.dll")]
        public static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName,
            uint nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(IntPtr hProcess,
            [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SizeT VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress,
            [Out] out MemoryBasicInformation lpBuffer, SizeT dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, SizeT dwSize, uint flAllocationType,
            MemPageProtect flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, SizeT dwSize, uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, SizeT dwSize,
            MemPageProtect flNewProtect, [Out] out MemPageProtect lpflOldProtect);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern IntPtr NtSuspendProcess(IntPtr hProcess);

        [DllImport("ntdll.dll", SetLastError = true)]
        public static extern IntPtr NtResumeProcess(IntPtr hProcess);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, SizeT dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll")]
        public static extern uint WaitForSingleObject(IntPtr handle, uint time);

        [DllImport("kernel32.dll")]
        public static extern bool TerminateThread(IntPtr handle, uint code);

        // privileges
        public const int PROCESS_CREATE_THREAD = 0x0002;
        public const int PROCESS_QUERY_INFORMATION = 0x0400;
        public const int PROCESS_VM_OPERATION = 0x0008;
        public const int PROCESS_VM_WRITE = 0x0020;
        public const int PROCESS_VM_READ = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        public struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        [DllImport("dbghelp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymInitialize([In] IntPtr hProcess, [In, Optional] string UserSearchPath, [In] bool fInvadeProcess);

        [DllImport("dbghelp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.U8)]
        public static extern ulong SymLoadModuleEx([In] IntPtr hProcess, [In] IntPtr hFile, [In] string ImageName, [In] string ModuleName, [In, MarshalAs(UnmanagedType.U8)] ulong BaseOfDll, [In] uint DllSize, [In] IntPtr Data, [In] uint Flags);

        [DllImport("dbghelp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymEnumSymbols([In] IntPtr hProcess, [In] ulong BaseOfDll, [In, Optional] string Mask, [In] PSYM_ENUMERATESYMBOLS_CALLBACK EnumSymbolsCallback, [In, Optional] IntPtr UserContext);

        public delegate bool PSYM_ENUMERATESYMBOLS_CALLBACK([In] ref SYMBOL_INFO pSymInfo, [In] uint SymbolSize, [In, Optional] IntPtr UserContext);

        [DllImport("dbghelp.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SymCleanup([In] IntPtr hProcess);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SYMBOL_INFO
        {
            // The size of the structure, in bytes.
            public uint SizeOfStruct;

            // A unique value that identifies the type data that describes the symbol.
            public uint TypeIndex;

            // This member is reserved for system use.
            public ulong Reserved_0;

            // This member is reserved for system use.
            public ulong Reserved_1;

            // The unique value for the symbol.
            public uint Index;

            // The symbol size, in bytes.
            public uint Size;

            // The base address of the module that contains the symbol.
            public ulong ModBase;

            public uint Flags;

            // The value of a constant.
            public ulong Value;

            // The virtual address of the start of the symbol.
            public ulong Address;

            // The register.
            public uint Register;

            // The DIA scope.
            public uint Scope;

            // The PDB classification.
            public uint Tag;

            // The length of the name, in characters, not including the null-terminating character.
            public uint NameLen;

            // The size of the Name buffer, in characters.
            public uint MaxNameLen;

            // The name of the symbol.
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string Name;
        }

        public static SYMBOL_INFO[] AllSymbols(this Process process, ProcessModuleWow64Safe module, string symbol = "*")
        {
            var procHandle = process.Handle;

            if (!WinAPI.SymInitialize(procHandle, Path.GetDirectoryName(module.FileName), false))
                throw new Exception("Failed to initialize symbols.");

            var symbols = new List<SYMBOL_INFO>();

            try
            {
                if (WinAPI.SymLoadModuleEx(procHandle, IntPtr.Zero, module.ModuleName, null, (ulong)(module.BaseAddress), (uint)(module.ModuleMemorySize), IntPtr.Zero, 0) == 0)
                    throw new Exception("Failed to load module's symbols.");

                WinAPI.PSYM_ENUMERATESYMBOLS_CALLBACK callback = new(enumSyms);

                if (!WinAPI.SymEnumSymbols(procHandle, (ulong)(module.BaseAddress), symbol, callback, IntPtr.Zero))
                    throw new Exception("Failed to enumerate over module's symbols.");
            }
            finally
            {
                WinAPI.SymCleanup(procHandle);
            }

            return symbols.ToArray();

            bool enumSyms(ref SYMBOL_INFO pSymInfo, uint SymbolSizem, IntPtr UserContext)
            {
                symbols.Add(pSymInfo);
                return true;
            }
        }
    }
}
