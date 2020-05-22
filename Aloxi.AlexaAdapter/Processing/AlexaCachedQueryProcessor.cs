using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using ZoolWay.Aloxi.AlexaAdapter.Interface;

namespace ZoolWay.Aloxi.AlexaAdapter.Processing
{
    internal class AlexaCachedQueryProcessor : AlexaPassthroughProcessor
    {
        public override string Name => "AlexaCachedQuery";

        public override async Task<JObject> ProcessRequest(AlexaSmartHomeRequest request, ILambdaContext lambdaContext)
        {
            Log.Warn(lambdaContext, "Caching not yet implemented!");
            // TODO
            // check the cache
            // function is stateless, what media is used for the cache?

            var response = await base.ProcessRequest(request, lambdaContext);

            // TODO
            // update cache

            return response;
        }
    }
}
