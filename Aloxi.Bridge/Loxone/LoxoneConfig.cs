﻿using System;
using System.Collections.Immutable;
using System.ComponentModel;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    [ImmutableObject(true)]
    public class LoxoneConfig
    {
        public string Miniserver { get; }
        public string Username { get; }
        public string Password { get; }
        public ImmutableArray<string> IgnoreCategories { get; }

        public LoxoneConfig(string miniserver, string username, string password, ImmutableArray<string> ignoreCategories)
        {
            this.Miniserver = miniserver;
            this.Username = username;
            this.Password = password;
            this.IgnoreCategories = ignoreCategories;
        }
    }
}
