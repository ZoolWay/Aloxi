using System;
using System.Collections.Immutable;
using System.ComponentModel;

using Akka.Actor;

using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Mediation
{
    [ImmutableObject(true)]
    internal abstract class MediationMessage
    {
        public class Received : MediationMessage
        {
            public ImmutableArray<byte> Message { get; }
            public string Topic { get; }
            public byte QosLevel { get; }
            public bool DupFlag { get; }
            public bool Retain { get; }

            public Received(byte[] message, string topic, byte qosLevel, bool dupFlag, bool retain)
            {
                this.Message = message.ToImmutableArray();
                this.Topic = topic;
                this.QosLevel = qosLevel;
                this.DupFlag = dupFlag;
                this.Retain = retain;
            }
        }

        public class RegisterProcessor : MediationMessage
        {
            public AloxiMessageOperation Operation { get; }
            public IActorRef Processor { get; }

            public RegisterProcessor(AloxiMessageOperation operation, IActorRef processor)
            {
                this.Operation = operation;
                this.Processor = processor;
            }
        }

        public class Process : MediationMessage
        {
            public AloxiMessageOperation Operation { get; }
            public string Payload { get; }
            public string ResponseTopic { get; }

            public Process(AloxiMessageOperation operation, string payload, string responseTopic)
            {
                this.Operation = operation;
                this.Payload = payload;
                this.ResponseTopic = responseTopic;
            }
        }

        public class Publish : MediationMessage
        {
            public string Topic { get; }
            public AloxiMessageOperation Operation { get; }
            public string Payload { get; }
            public string ResponseTopic { get; }

            public Publish(string topic, AloxiMessageOperation operation, string payload, string responseTopic = null)
            {
                this.Topic = topic;
                this.Operation = operation;
                this.Payload = payload;
                this.ResponseTopic = responseTopic;
            }
        }

        public class PublishAlexaResponse : MediationMessage
        {
            public string SerializedResponse { get; }

            public PublishAlexaResponse(string serializedResponse)
            {
                this.SerializedResponse = serializedResponse;
            }
        }

        public class RequestState : MediationMessage
        {
        }

        public class RequestConnect : MediationMessage
        {
        }

        public class StateConnectionClosed : MediationMessage
        {
        }

        public class StateSubscribed : MediationMessage
        {
        }

        public class StateUnsubscribed : MediationMessage
        {
        }

        public class CurrentState : MediationMessage
        {
            public bool IsConnected { get; }
            public bool IsSubscribed { get; }
            public DateTime Timestamp { get; }

            public CurrentState(bool isConnected, bool isSubscribed, DateTime timestamp)
            {
                this.IsConnected = isConnected;
                this.IsSubscribed = isSubscribed;
                this.Timestamp = timestamp;
            }
        }
    }
}
