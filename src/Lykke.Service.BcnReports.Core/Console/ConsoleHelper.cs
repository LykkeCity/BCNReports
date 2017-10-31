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
            console.WriteLine($"{component}:{process} {info}");
        }
    }
}
