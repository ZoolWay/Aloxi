using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using ZoolWay.Aloxi.AlexaAdapter.Interface;

namespace ZoolWay.Aloxi.AlexaAdapter.Processing
{
    internal class AlexaPassthroughProcessor : AbstractProcessor
    {
        public override string Name => "AlexaPassthrough";

        public override Task<JObject> ProcessRequest(AlexaSmartHomeRequest request, ILambdaContext lambdaContext)
        {
            var config = Configuration.ProvideFor(lambdaContext);
            return PerformPassthroughRequest(request, config, lambdaContext);
        }
    }
}
