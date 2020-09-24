using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ZoolWay.Aloxi.AlexaAdapter.Interface;

namespace ZoolWay.Aloxi.AlexaAdapter.Processing
{
    internal abstract class AbstractProcessor
    {
        protected readonly JsonSerializerSettings jsonSettings;
        protected readonly JsonSerializer json;

        public abstract string Name { get; }

        protected AbstractProcessor()
        {
            this.jsonSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            this.json = JsonSerializer.CreateDefault(this.jsonSettings);
        }

        public abstract Task<JObject> ProcessRequest(AlexaSmartHomeRequest request, ILambdaContext lambdaContext);

        protected async Task<JObject> PerformPassthroughRequest(AlexaSmartHomeRequest request, Configuration config, ILambdaContext lambdaContext)
        {
            var client = new PubSubClient(config, lambdaContext);

            Log.Debug(lambdaContext, "Starting passthrough...");
            var sw = Stopwatch.StartNew();
            JObject response = await client.RequestBridgePassthrough(Meta.AloxiMessageOperation.PipeAlexaRequest, JObject.FromObject(request, this.json));
            sw.Stop();

            if (response == null)
            {
                Log.Warn(lambdaContext, $"Passthrough completed in {sw.Elapsed.TotalSeconds}s without result");
            }
            else
            {
                Log.Info(lambdaContext, $"Successful passthrough, took {sw.Elapsed.TotalSeconds}s");
            }
            return response;
        }

        protected JObject CreateResponse(object response)
        {
            return JObject.FromObject(response, this.json);
        }

    }
}
