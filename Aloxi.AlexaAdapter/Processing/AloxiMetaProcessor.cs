using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using ZoolWay.Aloxi.AlexaAdapter.Processing.Meta;

namespace ZoolWay.Aloxi.AlexaAdapter.Processing
{
    internal class AloxiMetaProcessor : AbstractProcessor
    {
        public override string Name => "AloxiMeta";

        public override Task<JObject> ProcessRequest(string requestName, JObject payload, ILambdaContext lambdaContext)
        {
            var config = Configuration.ProvideFor(lambdaContext);

            switch (requestName)
            {
                case "EchoRequest":
                    return PerformEchoRequest(payload, config, lambdaContext);
            }

            Log.Error($"Request '{requestName}' is unkown");
            return Task.FromResult(JObject.FromObject(new { Message = $"Failed, {requestName} is unknown" }));
        }

        private async Task<JObject> PerformEchoRequest(JObject payload, Configuration config, ILambdaContext lambdaContext)
        {
            var client = new PubSubClient(config);

            var sw = Stopwatch.StartNew();
            EchoPayload outgoingEcho = new EchoPayload() { Salt = Guid.NewGuid().ToString() };
            EchoPayload response = await client.RequestBridge<EchoPayload>(AloxiMessageOperation.Echo, outgoingEcho);
            sw.Stop();

            if (!(response?.Salt == outgoingEcho.Salt)) throw new Exception("Echo response invalid");

            Log.Info($"Successful echo, took {sw.Elapsed.TotalSeconds}s");
            return CreateResponse(new { Message = "Echo fine", DurationInSeconds = sw.Elapsed.TotalSeconds, Salt = response.Salt });
        }
    }
}
