using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using ZoolWay.Aloxi.Bridge.Mqtt;

namespace ZoolWay.Aloxi.Bridge.Alexa
{
    public class AdapterActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);

        public AdapterActor()
        {
            Receive<MqttMessage.Process>(ReceivedProcess);
        }

        private void ReceivedProcess(MqttMessage.Process message)
        {
            throw new NotImplementedException();
        }
    }
}
