using System;
using System.Collections.Generic;

using Akka.Actor;
using Akka.Event;

namespace ZoolWay.Aloxi.Bridge.Mediation.SignalR
{
    public class ManagerActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Context.GetLogger();
        private readonly Config signalRConfig;
        private readonly HashSet<AloxiHub> registeredHubs;
        private readonly List<MediationMessage> registeredProcessors;

        public ManagerActor(Config signalRConfig)
        {
            this.signalRConfig = signalRConfig;
            this.registeredHubs = new HashSet<AloxiHub>();
            this.registeredProcessors = new List<MediationMessage>();

            Receive<MediationMessage.RegisterProcessor>(ReceivedRegisterProcessor);
            Receive<SignalRMessage.RegisterHub>(ReceivedRegisterHub);
            Receive<SignalRMessage.UnregisterHub>(ReceivedUnregisterHub);
        }

        private void ReceivedRegisterProcessor(MediationMessage.RegisterProcessor message)
        {
            log.Info($"Registering processor for op '{message.Operation}': {message.Processor.Path.ToStringWithoutAddress()}");
            this.registeredProcessors.Add(message);
        }

        private void ReceivedUnregisterHub(SignalRMessage.UnregisterHub message)
        {
            this.registeredHubs.Remove(message.Hub);
        }

        private void ReceivedRegisterHub(SignalRMessage.RegisterHub message)
        {
            this.registeredHubs.Add(message.Hub);
        }
    }
}
