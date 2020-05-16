using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ZoolWay.AloxiAlexaAdapter.Processing
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

        public abstract Task<JObject> ProcessRequest(string requestName, JObject payload, ILambdaContext lambdaContext);

        protected JObject CreateResponse(object response)
        {
            return JObject.FromObject(response, this.json);
        }

    }
}
