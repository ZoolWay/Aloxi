using System;
using System.ComponentModel;
using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Bus
{
    [ImmutableObject(true)]
    public class HomeModelUpdatedEvent
    {
        public Home HomeModel { get; }

        public HomeModelUpdatedEvent(Home updatedHomeModel)
        {
            this.HomeModel = updatedHomeModel;
        }
    }
}
