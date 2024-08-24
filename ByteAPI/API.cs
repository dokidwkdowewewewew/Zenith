

using DllInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

#nullable enable
namespace ZenithAPI
{
    public class API
    {
        public static API.InjectionStatus injectionStatus = API.InjectionStatus.NOT_INJECTED;
        private static int port = 9912;

        public static string GUID() => File.ReadAllText("C:\\ProgramData\\Zenith\\guid");

        public static DllInjectionResult Initialize(string bootstrapperLocation, string dll)
        {
            if (!File.Exists(bootstrapperLocation))
                return DllInjectionResult.NoBootstrapper;

            Process.Start(bootstrapperLocation);

            while (true)
            {
                if (Process.GetProcessesByName("Zenith").Length == 0)
                    Thread.Sleep(20);
                else
                    break;
            }

            Thread.Sleep(4000);

            if (!Directory.Exists("C:\\ProgramData\\Zenith"))
                return DllInjectionResult.InjectionFailed;

            if (File.Exists("C:\\ProgramData\\Zenith\\guid"))
                File.Delete("C:\\ProgramData\\Zenith\\guid");

            DllInjectionResult dllInjectionResult = DllInjector.Inject("Zenith", dll);
            Thread.Sleep(2000);
            return dllInjectionResult;
        }

        public static API.InjectionResult Inject(string bootstrapperLocation, string dll)
        {
            if (API.injectionStatus == API.InjectionStatus.INJECTED)
                return API.InjectionResult.ALREADY_INJECTED;

            if (API.injectionStatus == API.InjectionStatus.INJECTING)
                return API.InjectionResult.ALREADY_INJECTING;

            API.injectionStatus = API.InjectionStatus.INJECTING;

            if (API.Initialize(bootstrapperLocation, dll) != DllInjectionResult.Success)
                return API.InjectionResult.GUID_DUMP_FAIL;

            Thread.Sleep(300);

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    string address = string.Format("http://localhost:{0}/request", (object)API.port);
                    webClient.Headers.Add("Content-Type", "plain/text");
                    webClient.Headers.Add("User-Agent", "Zenith/v3.0");
                    webClient.Headers.Add("GUID", API.GUID());

                    switch (webClient.UploadString(address, "POST", "Attach"))
                    {
                        case ".":
                            API.injectionStatus = API.InjectionStatus.NOT_INJECTED;
                            return API.InjectionResult.ALREADY_INJECTED;
                        case ";":
                            API.injectionStatus = API.InjectionStatus.NOT_INJECTED;
                            return API.InjectionResult.APPLICATION_NOT_OPEN;
                        case "-":
                            API.injectionStatus = API.InjectionStatus.NOT_INJECTED;
                            return API.InjectionResult.FAILED;
                        case ">":
                            API.injectionStatus = API.InjectionStatus.NOT_INJECTED;
                            return API.InjectionResult.APPLICATION_VERSION_MISMATCH;
                        default:
                            API.injectionStatus = API.InjectionStatus.INJECTED;
                            File.WriteAllText("C:\\ProgramData\\Zenith\\Message", "kys");
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
                    webClient.Encoding = Encoding.UTF8;
                    string address = string.Format("http://localhost:{0}/request", (object)API.port);
                    webClient.Headers.Add("Content-Type", "plain/text");
                    webClient.Headers.Add("User-Agent", "Zenith/v3.0");
                    webClient.Headers.Add("GUID", API.GUID());
                    webClient.UploadString(address, "POST", "ExecuteScript:" + script);
                }
                return true;
            }
            catch
            {
                return false;
            }
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
            APPLICATION_NOT_OPEN,
            APPLICATION_VERSION_MISMATCH,
        }

        public enum InjectionStatus
        {
            NOT_INJECTED,
            INJECTING,
            INJECTED,
        }
    }
}

  
