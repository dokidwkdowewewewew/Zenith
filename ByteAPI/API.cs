// Decompiled with JetBrains decompiler
// Type: ByteAPI.API
// Assembly: ByteAPI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 90DBCC87-1BB9-42AD-BA95-770989B46DFE
// Assembly location: C:\Users\notjo\Desktop\ByteAPI\ByteAPI\ByteAPI.dll

using DllInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

#nullable enable
namespace ByteAPI
{
  public class API
  {
    public static API.InjectionStatus injectionStatus = API.InjectionStatus.NOT_INJECTED;
    private static int port = 9912;

    public static string GUID() => File.ReadAllText("C:\\ProgramData\\Solara\\guid.txt");

    public static DllInjectionResult Initialize(string BootstrapperLocation, string dll)
    {
      if (!File.Exists(BootstrapperLocation))
        return DllInjectionResult.NoBootstrapper;
      Process.Start(BootstrapperLocation);
      while (true)
      {
        if (Process.GetProcessesByName("Solara").Length == 0)
          Thread.Sleep(20);
        else
          break;
      }
      Thread.Sleep(4000);
      if (!Directory.Exists("C:\\ProgramData\\Solara"))
        return DllInjectionResult.InjectionFailed;
      if (File.Exists("C:\\ProgramData\\Solara\\guid.txt"))
        File.Delete("C:\\ProgramData\\Solara\\guid.txt");
      DllInjectionResult dllInjectionResult = DllInjector.Inject("Solara", dll);
      Thread.Sleep(2000);
      return dllInjectionResult;
    }

    public static API.InjectionResult Inject(string bootstrapperlocation, string dll)
    {
      if (API.injectionStatus == API.InjectionStatus.INJECTED)
        return API.InjectionResult.ALREADY_INJECTED;
      if (API.injectionStatus == API.InjectionStatus.INJECTING)
        return API.InjectionResult.ALREADY_INJECTING;
      API.injectionStatus = API.InjectionStatus.INJECTING;
      if (API.Initialize(bootstrapperlocation, dll) != DllInjectionResult.Success)
        return API.InjectionResult.GUID_DUMP_FAIL;
      Thread.Sleep(300);
      try
      {
        using (WebClient webClient = new WebClient())
        {
          webClient.Encoding = Encoding.UTF8;
          string address = string.Format("http://localhost:{0}/request", (object) API.port);
          webClient.Headers.Add("Content-Type", "plain/text");
          webClient.Headers.Add("User-Agent", "Solara/v3.0");
          webClient.Headers.Add("GUID", API.GUID());
          switch (webClient.UploadString(address, "POST", "Attach"))
          {
            case ".":
              API.injectionStatus = API.InjectionStatus.NOT_INJECTED;
              return API.InjectionResult.ALREADY_INJECTED;
            case ";":
              API.injectionStatus = API.InjectionStatus.NOT_INJECTED;
              return API.InjectionResult.ROBLOX_NOT_OPEN;
            case "-":
              API.injectionStatus = API.InjectionStatus.NOT_INJECTED;
              return API.InjectionResult.FAILED;
            case ">":
              API.injectionStatus = API.InjectionStatus.NOT_INJECTED;
              return API.InjectionResult.ROBLOX_VERSION_MISMATCH;
            default:
              API.injectionStatus = API.InjectionStatus.INJECTED;
              File.WriteAllText("C:\\ProgramData\\Solara\\FuckYouSolara.txt", "kys");
              return API.InjectionResult.SUCCESS;
          }
        }
      }
      catch
      {
        return API.InjectionResult.ACCESS_DENIED;
      }
    }

    public static bool Execute(string script)
    {
      try
      {
        using (WebClient webClient = new WebClient())
        {
          string str = script;
          webClient.Encoding = Encoding.UTF8;
          string address = string.Format("http://localhost:{0}/request", (object) API.port);
          webClient.Headers.Add("Content-Type", "plain/text");
          webClient.Headers.Add("User-Agent", "Solara/v3.0");
          webClient.Headers.Add("GUID", API.GUID());
          webClient.UploadString(address, "POST", "ExecuteScript:" + str);
        }
        return true;
      }
      catch
      {
        return false;
      }
    }

    public static void GetKey()
    {
      string str = new WebClient().DownloadString("https://raw.githubusercontent.com/danny-125/byte/main/link.txt");
      Process process = new Process();
      try
      {
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = str;
        process.Start();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    public static void EnterKey(string key)
    {
      if (File.Exists("C:\\ProgramData\\Solara\\key.txt"))
        File.Delete("C:\\ProgramData\\Solara\\key.txt");
      File.WriteAllText("C:\\ProgramData\\Solara\\key.txt", key);
    }

    public enum InjectionResult
    {
      SUCCESS,
      FAILED,
      ACCESS_DENIED,
      GUID_DUMP_FAIL,
      DLL_MISSING,
      ALREADY_INJECTED,
      ALREADY_INJECTING,
      ROBLOX_NOT_OPEN,
      ROBLOX_VERSION_MISMATCH,
    }

    public enum InjectionStatus
    {
      NOT_INJECTED,
      INJECTING,
      INJECTED,
    }
  }
}
