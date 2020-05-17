using System;
using Akka.Actor;
using Akka.Event;
using ZoolWay.Aloxi.Bridge.Models;
using ZoolWay.Aloxi.Bridge.Mqtt;

namespace ZoolWay.Aloxi.Bridge.Meta
{
    public class MetaProcessorActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly IActorRef publisher;

        public MetaProcessorActor(IActorRef publisher)
        {
            this.publisher = publisher;
            Receive<MqttMessage.Process>(ReceivedProcess);
        }

        private void ReceivedProcess(MqttMessage.Process message)
        {
            if (message.Operation == AloxiMessageOperation.Echo)
            {
                this.publisher.Tell(new MqttMessage.Publish(message.ResponseTopic, AloxiMessageOperation.EchoResponse, message.Payload));
            }
        }
    }
}
