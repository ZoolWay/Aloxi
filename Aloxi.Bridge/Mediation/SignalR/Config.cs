using System;
using System.ComponentModel;

namespace ZoolWay.Aloxi.Bridge.Mediation.SignalR
{
    [ImmutableObject(true)]
    public class Config
    {
        public string ConnectionString { get; private set; }

        public Config(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
    }
}
