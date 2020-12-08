using System;

using Akka.Actor;
using Akka.Event;

using ZoolWay.Aloxi.Bridge.Mediation;
using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Meta
{
    public class MetaProcessorActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly IActorRef publisher;

        public MetaProcessorActor(IActorRef publisher)
        {
            this.publisher = publisher;
            Receive<MediationMessage.Process>(ReceivedProcess);
        }

        private void ReceivedProcess(MediationMessage.Process message)
        {
            if (message.Operation == AloxiMessageOperation.Echo)
            {
                this.publisher.Tell(new MediationMessage.Publish(message.ResponseTopic, AloxiMessageOperation.EchoResponse, message.Payload));
            }
        }
    }
}
