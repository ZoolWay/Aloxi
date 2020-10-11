using System;
using System.Collections.Generic;

namespace ZoolWay.Aloxi.Bridge.Alexa.Models.Impl
{
    internal class ModeControllerCapabilityForBlinds : ModeControllerCapability
    {
        public override string Instance { get => "Blinds.BlindTargetState"; }
        public ModeControllerCapabilityConfiguration Configuration { get; set; }

        public ModeControllerCapabilityForBlinds()
        {
            this.CapabilityResources = new AlexaCapabilityResources()
            {
                FriendlyNames = new List<AlexaCapabilityFriendlyName>()
                {
                    new AlexaCapabilityFriendlyName("asset", "Alexa.Setting.Opening"),
                },
            };
            this.Configuration = new ModeControllerCapabilityConfiguration()
            {
                Ordered = false,
                SupportedModes = new List<ModeControllerCapabilityConfiguration.ModeValue>()
                {
                    new ModeControllerCapabilityConfiguration.ModeValue()
                    {
                        Value = "BlindTargetState.FullUp",
                        ModeResources = new ModeControllerCapabilityConfiguration.ModeResources()
                        {
                            FriendlyNames = new List<AlexaCapabilityFriendlyName>()
                            {
                                new AlexaCapabilityFriendlyName("asset", "Alexa.Value.Open"),
                            },
                        },
                    },
                    new ModeControllerCapabilityConfiguration.ModeValue()
                    {
                        Value = "BlindTargetState.FullDown",
                        ModeResources = new ModeControllerCapabilityConfiguration.ModeResources()
                        {
                            FriendlyNames = new List<AlexaCapabilityFriendlyName>()
                            {
                                new AlexaCapabilityFriendlyName("asset", "Alexa.Value.Close"),
                            },
                        },
                    },
                    new ModeControllerCapabilityConfiguration.ModeValue()
                    {
                        Value = "BlindTargetState.Stop",
                        ModeResources = new ModeControllerCapabilityConfiguration.ModeResources()
                        {
                            FriendlyNames = new List<AlexaCapabilityFriendlyName>()
                            {
                                new AlexaCapabilityFriendlyName("text", "stoppe", "de-DE"),
                            },
                        },
                    },
                }
            };
            this.Semantics = new AlexaCapabilitySemantics()
            {
                ActionMappings = new List<AlexaCapabilitySemanticActionMapping>()
                {
                    new AlexaCapabilitySemanticActionMapping()
                    {
                        Type = "ActionsToDirective",
                        Actions = new [] { "Alexa.Actions.Close", "Alexa.Actions.Lower" },
                        Directive = new AlexaCapabilitySemanticActionMapping.AmDirective("SetMode", "BlindTargetState.FullDown"),
                    },
                    new AlexaCapabilitySemanticActionMapping()
                    {
                        Type = "ActionsToDirective",
                        Actions = new [] { "Alexa.Actions.Open", "Alexa.Actions.Raise" },
                        Directive  = new AlexaCapabilitySemanticActionMapping.AmDirective("SetMode", "BlindTargetState.FullUp")
                    },
                },
                StateMappings = new List<AlexaCapabilitySemanticStateMapping>()
                {
                    new AlexaCapabilitySemanticStateMapping()
                    {
                        Type = "StatesToValue",
                        States = new [] { "Alexa.States.Closed" },
                        Value = "BlindTargetState.FullDown",
                    },
                    new AlexaCapabilitySemanticStateMapping()
                    {
                        Type = "StatesToValue",
                        States = new [] { "Alexa.States.Open" },
                        Value = "BlindTargetState.FullUp",
                    },
                },
            };
        }
    }
}
