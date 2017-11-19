using System;
using System.Collections.Generic;
using System.Text;
using Common.Log;

namespace Lykke.Service.BcnReports.Core.Console
{
    public static class ConsoleHelper
    {
        public static void WriteConsoleLog(this IConsole console, string component, string process, string info)
        {
            WriteConsoleLog(console, component, process, null, info);
        }

        public static void WriteConsoleLog(this IConsole console, string component, string process, string context, string info)
        {

            console.WriteLine($"{DateTime.Now} :: {component}.{process}:: {info} :: {context}");
        }
    }
}
