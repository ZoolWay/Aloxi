using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using ZoolWay.Aloxi.AlexaAdapter.Interface;
using ZoolWay.Aloxi.AlexaAdapter.Processing.Meta;

namespace ZoolWay.Aloxi.AlexaAdapter.Processing
{
    internal class AloxiMetaProcessor : AbstractProcessor
    {
        public override string Name => "AloxiMeta";

        public override Task<JObject> ProcessRequest(AlexaSmartHomeRequest request, ILambdaContext lambdaContext)
        {
            var config = Configuration.ProvideFor(lambdaContext);

            switch (request.Directive.Header.Name)
            {
                case "EchoRequest":
                    return PerformEchoRequest(request.Directive.Payload, config, lambdaContext);
            }

            Log.Error(lambdaContext, $"Request '{request.Directive.Header.Name}' is unkown");
            return Task.FromResult(JObject.FromObject(new { Message = $"Failed, {request.Directive.Header.Name} is unknown" }));
        }

        private async Task<JObject> PerformEchoRequest(JObject payload, Configuration config, ILambdaContext lambdaContext)
        {
            var client = new PubSubClient(config, lambdaContext);

            var sw = Stopwatch.StartNew();
            EchoPayload outgoingEcho = new EchoPayload() { Salt = Guid.NewGuid().ToString() };
            EchoPayload response = await client.RequestBridge<EchoPayload>(AloxiMessageOperation.Echo, outgoingEcho);
            sw.Stop();

            if (!(response?.Salt == outgoingEcho.Salt)) throw new Exception("Echo response invalid");

            Log.Info(lambdaContext, $"Successful echo, took {sw.Elapsed.TotalSeconds}s");
            return CreateResponse(new { Message = "Echo fine", DurationInSeconds = sw.Elapsed.TotalSeconds, Salt = response.Salt });
        }
    }
}
