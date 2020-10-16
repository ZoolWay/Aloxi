using System;
using System.Collections.Immutable;
using System.ComponentModel;
using Akka.Actor;
using Akka.Event;
using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Mqtt
{
    [ImmutableObject(true)]
    internal abstract class MqttMessage
    {
        public class Received : MqttMessage
        {
            public ImmutableArray<byte> Message { get; }
            public string Topic { get; }
            public byte QosLevel { get; }
            public bool DupFlag { get; }
            public bool Retain { get; }

            public Received(byte[] message, string topic, byte qosLevel, bool dupFlag, bool retain)
            {
                this.Message = message.ToImmutableArray<byte>();
                this.Topic = topic;
                this.QosLevel = qosLevel;
                this.DupFlag = dupFlag;
                this.Retain = retain;
            }
        }

        public class RegisterProcessor : MqttMessage
        {
            public AloxiMessageOperation Operation { get; }
            public IActorRef Processor { get; }

            public RegisterProcessor(AloxiMessageOperation operation, IActorRef processor)
            {
                this.Operation = operation;
                this.Processor = processor;
            }
        }

        public class Process : MqttMessage
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

        public class Publish : MqttMessage
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

        public class PublishAlexaResponse : MqttMessage
        {
            public string SerializedResponse { get; }

            public PublishAlexaResponse(string serializedResponse)
            {
                this.SerializedResponse = serializedResponse;
            }
        }

        public class RequestState : MqttMessage
        {
        }

        public class StateConnectionClosed : MqttMessage
        {
        }

        public class StateSubscribed : MqttMessage
        {
        }

        public class StateUnsubscribed : MqttMessage
        {
        }

        public class CurrentState : MqttMessage
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
