using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ZoolWay.Aloxi.Bridge.Mediation.Mqtt
{
    [ImmutableObject(true)]
    internal abstract class MqttMessage
    {
        public class EstablishSubscription : MediationMessage
        {
        }
    }
}
