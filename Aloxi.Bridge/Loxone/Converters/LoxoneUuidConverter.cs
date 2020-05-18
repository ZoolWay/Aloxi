using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace ZoolWay.Aloxi.Bridge.Loxone.Converters
{
    public class LoxoneUuidConverter : JsonConverter<LoxoneUuid>
    {
        public override LoxoneUuid ReadJson(JsonReader reader, Type objectType, [AllowNull] LoxoneUuid existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return LoxoneUuid.Parse(s);
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] LoxoneUuid value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
