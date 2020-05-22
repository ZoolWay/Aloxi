using System;
using System.Diagnostics;
using System.Linq;
using Amazon.Lambda.Core;

namespace ZoolWay.Aloxi.AlexaAdapter
{
    public static class Log
    {
        private const string LV_ERROR = "ERROR";
        private const string LV_INFO = " INFO";
        private const string LV_WARN = " WARN";
        private const string LV_DEBUG = "DEBUG";
        private static string configuredLevel = InitConfiguredLogLevel();

        public static void Error(ILambdaContext context, string message)
        {
            LambdaLog(context, LV_ERROR, message);
            TraceLog(LV_ERROR, message);
        }

        public static void Info(ILambdaContext context, string message)
        {
            LambdaLog(context, LV_INFO, message);
            TraceLog(LV_INFO, message);
        }

        public static void Warn(ILambdaContext context, string message)
        {
            LambdaLog(context, LV_WARN, message);
            TraceLog(LV_WARN, message);
        }

        public static void Debug(ILambdaContext context, string message)
        {
            LambdaLog(context, LV_DEBUG, message);
            TraceLog(LV_DEBUG, message);
        }

        private static void LambdaLog(ILambdaContext context, string level, string message)
        {
            if ((level == "DEBUG") && (!LevelMatches("DEBUG", "INFO", "WARN", "ERROR"))) return;
            if ((level == "INFO") && (!LevelMatches("INFO", "WARN", "ERROR"))) return;
            if ((level == "WARN") && (!LevelMatches("WARN", "ERROR"))) return;
            if ((level == "ERROR") && (!LevelMatches("ERROR"))) return;
            if (String.IsNullOrWhiteSpace(context?.AwsRequestId))
            {
                LambdaLogger.Log($"{level.ToUpper()}  {message}\n");
            }
            else
            {
                LambdaLogger.Log($"{level.ToUpper()} RequestId: {context.AwsRequestId}  {message}\n");
            }
        }

        [Conditional("TRACE")]
        private static void TraceLog(string level, string message)
        {
            Trace.WriteLine($"[{DateTime.Now.TimeOfDay.TotalSeconds:F1}][{level.ToUpper()}] {message}");
        }

        private static string InitConfiguredLogLevel()
        {
            return Environment.GetEnvironmentVariable("LAMBDA_LOG") ?? "WARN";
        }

        private static bool LevelMatches(params string[] acceptLevels)
        {
            return acceptLevels.Contains(configuredLevel);
        }

    }
}
