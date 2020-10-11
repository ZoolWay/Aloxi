using System;

using ZoolWay.Aloxi.Bridge.Loxone;

namespace ZoolWay.Aloxi.Bridge.Alexa
{
    public static class AlexaUuidTranslator
    {
        public static string ToAlexaId(LoxoneUuid loxoneUuid)
        {
            return loxoneUuid.ToString().Replace("/", "--");
        }

        public static LoxoneUuid ToLoxoneUuid(string alexaId)
        {
            return LoxoneUuid.Parse(alexaId.Replace("--", "/"));
        }
    }
}
