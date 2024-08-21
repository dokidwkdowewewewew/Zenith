// Decompiled with JetBrains decompiler
// Type: DllInjection.DllInjector
// Assembly: ByteAPI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 90DBCC87-1BB9-42AD-BA95-770989B46DFE
// Assembly location: C:\Users\notjo\Desktop\ByteAPI\ByteAPI\ByteAPI.dll

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
    private static readonly IntPtr INTPTR_ZERO;
    private static DllInjector _instance;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(
      uint dwDesiredAccess,
      int bInheritHandle,
      uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(
      IntPtr hProcess,
      IntPtr lpAddress,
      IntPtr dwSize,
      uint flAllocationType,
      uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int WriteProcessMemory(
      IntPtr hProcess,
      IntPtr lpBaseAddress,
      byte[] buffer,
      uint size,
      int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateRemoteThread(
      IntPtr hProcess,
      IntPtr lpThreadAttribute,
      IntPtr dwStackSize,
      IntPtr lpStartAddress,
      IntPtr lpParameter,
      uint dwCreationFlags,
      IntPtr lpThreadId);

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

    private static unsafe bool bInject(uint pToBeInjected, string sDllPath)
    {
      IntPtr num1 = DllInjector.OpenProcess(1082U, 1, pToBeInjected);
      if (num1 == DllInjector.INTPTR_ZERO)
        return false;
      IntPtr procAddress = DllInjector.GetProcAddress(DllInjector.GetModuleHandle("kernel32.dll"), "LoadLibraryA");
      if (procAddress == DllInjector.INTPTR_ZERO)
        return false;
      IntPtr num2 = DllInjector.VirtualAllocEx(num1, (IntPtr) (void*) null, (IntPtr) sDllPath.Length, 12288U, 64U);
      if (num2 == DllInjector.INTPTR_ZERO)
        return false;
      byte[] bytes = Encoding.ASCII.GetBytes(sDllPath);
      if (DllInjector.WriteProcessMemory(num1, num2, bytes, (uint) bytes.Length, 0) == 0 || DllInjector.CreateRemoteThread(num1, (IntPtr) (void*) null, DllInjector.INTPTR_ZERO, procAddress, num2, 0U, (IntPtr) (void*) null) == DllInjector.INTPTR_ZERO)
        return false;
      DllInjector.CloseHandle(num1);
      return true;
    }
  }
}
