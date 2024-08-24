using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

#nullable enable
namespace DllInjection
{
    public sealed class DllInjector
    {
        private static readonly IntPtr INTPTR_ZERO = IntPtr.Zero;
        private static DllInjector _instance;

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtCreateThreadEx(
            out IntPtr hThread,
            uint dwDesiredAccess,
            IntPtr lpObjectAttributes,
            IntPtr hProcess,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            bool bCreateSuspended,
            uint dwStackSize,
            uint dwMaxStackSize,
            uint dwSectionHandle,
            IntPtr lpReserved);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtWriteVirtualMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] buffer,
            uint size,
            out uint lpNumberOfBytesWritten);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtAllocateVirtualMemory(
            IntPtr hProcess,
            ref IntPtr lpBaseAddress,
            uint ZeroBits,
            ref uint RegionSize,
            uint AllocationType,
            uint Protect);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtQueryInformationProcess(
            IntPtr ProcessHandle,
            uint ProcessInformationClass,
            out IntPtr ProcessInformation,
            uint ProcessInformationLength,
            out uint ReturnLength);

        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        private const uint MEM_COMMIT = 0x1000;
        private const uint PAGE_READWRITE = 0x04;
        private const uint THREAD_ALL_ACCESS = 0x001F03FF;

        public static DllInjector GetInstance
        {
            get
            {
                if (DllInjector._instance == null)
                    DllInjector._instance = new DllInjector();
                return DllInjector._instance;
            }
        }

        private DllInjector()
        {
        }

        public static DllInjectionResult Inject(string sProcName, string sDllPath)
        {
            if (!File.Exists(sDllPath))
                return DllInjectionResult.DllNotFound;
            uint pToBeInjected = 0;
            Process[] processes = Process.GetProcesses();
            for (int index = 0; index < processes.Length; ++index)
            {
                if (processes[index].ProcessName == sProcName)
                {
                    pToBeInjected = (uint) processes[index].Id;
                    break;
                }
            }
            if (pToBeInjected == 0U)
                return DllInjectionResult.GameProcessNotFound;
            return !DllInjector.bInject(pToBeInjected, sDllPath) ? DllInjectionResult.InjectionFailed : DllInjectionResult.Success;
        }

        private static bool bInject(uint pToBeInjected, string sDllPath)
        {
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, pToBeInjected);
            if (hProcess == INTPTR_ZERO)
                return false;

            IntPtr pRemoteAddress = INTPTR_ZERO;
            uint dwSize = (uint)sDllPath.Length;
            int status = NtAllocateVirtualMemory(hProcess, ref pRemoteAddress, 0, ref dwSize, MEM_COMMIT, PAGE_READWRITE);
            if (status != 0 || pRemoteAddress == INTPTR_ZERO)
                return false;

            byte[] bytes = Encoding.ASCII.GetBytes(sDllPath);
            uint bytesWritten;
            status = NtWriteVirtualMemory(hProcess, pRemoteAddress, bytes, (uint)bytes.Length, out bytesWritten);
            if (status != 0 || bytesWritten != (uint)bytes.Length)
                return false;

            IntPtr hThread;
            status = NtCreateThreadEx(out hThread, THREAD_ALL_ACCESS, INTPTR_ZERO, hProcess, GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA"), pRemoteAddress, false, 0, 0, 0, INTPTR_ZERO);
            if (status != 0 || hThread == INTPTR_ZERO)
                return false;

            CloseHandle(hProcess);
            return true;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, uint bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
