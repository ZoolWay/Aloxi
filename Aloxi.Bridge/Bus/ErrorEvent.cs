using System;
using System.ComponentModel;

namespace ZoolWay.Aloxi.Bridge.Bus
{
    [ImmutableObject(true)]
    public class ErrorEvent
    {
        public string Message { get; }
        public string Source { get; }
        public ErrorSeverity Severity { get; }

        public ErrorEvent(string message, string source, ErrorSeverity severity)
        {
            this.Message = message;
            this.Source = source;
            this.Severity = severity;
        }
    }
}
