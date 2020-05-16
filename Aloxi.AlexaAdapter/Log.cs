using System;
using System.Diagnostics;
using Amazon.Lambda.Core;

namespace ZoolWay.Aloxi.AlexaAdapter
{
    public static class Log
    {
        public static void Error(string message)
        {
            LambdaLogger.Log(message);
            Trace.WriteLine($"[{DateTime.Now.TimeOfDay.TotalSeconds:F1}][ERROR] {message}");
        }

        public static void Info(string message)
        {
            LambdaLogger.Log(message);
            Trace.WriteLine($"[{DateTime.Now.TimeOfDay.TotalSeconds:F1}][INFO] {message}");
        }

        public static void Debug(string message)
        {
            Trace.WriteLine($"[{DateTime.Now.TimeOfDay.TotalSeconds:F1}][DEBUG] {message}");
        }
    }
}
