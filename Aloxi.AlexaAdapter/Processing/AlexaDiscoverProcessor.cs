using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

namespace ZoolWay.Aloxi.AlexaAdapter.Processing
{
    internal class AlexaDiscoverProcessor : AbstractProcessor
    {
        public override string Name => "AlexaDiscover";

        public override Task<JObject> ProcessRequest(string requestName, JObject payload, ILambdaContext lambdaContext)
        {
            var config = Configuration.ProvideFor(lambdaContext);

            switch (requestName)
            {
                case "DiscoverAppliancesRequest":
                    return PerformDiscoverAppliancesRequest(payload, config, lambdaContext);
            }

            Log.Error($"Request '{requestName}' is unkown");
            return Task.FromResult(JObject.FromObject(new { Message = $"Failed, {requestName} is unknown" }));
        }

        private async Task<JObject> PerformDiscoverAppliancesRequest(JObject payload, Configuration config, ILambdaContext lambdaContext)
        {
            var client = new PubSubClient(config);

            var sw = Stopwatch.StartNew();
            JObject response = await client.RequestBridgePassthrough(Meta.AloxiMessageOperation.PipeAlexaRequest, payload);
            sw.Stop();

            Log.Info($"Successful discover, took {sw.Elapsed.TotalSeconds}s");
            return CreateResponse(response);
        }
    }
}
