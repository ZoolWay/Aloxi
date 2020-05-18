using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;
using ZoolWay.Aloxi.AlexaAdapter.Interface;
using ZoolWay.Aloxi.AlexaAdapter.Processing;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace ZoolWay.Aloxi.AlexaAdapter
{
    /// <summary>
    /// Main function which handles all Aloxi-related requests coming from Alexa or testing clients.
    /// </summary>
    public class Function
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<object> FunctionHandler(JObject request, ILambdaContext context)
        {
            if (request == null) throw new Exception("No request data given");

            var alexaRequest = request.ToObject<AlexaSmartHomeRequest>();
            if (alexaRequest.Header == null) throw new Exception("Invalid request, header missing");
            Log.Debug($"Checking request in namespace {alexaRequest.Header.Namespace}");

            AbstractProcessor processor = null;
            switch (alexaRequest.Header.Namespace)
            {
                case "Aloxi.MetaControl":
                    processor = new AloxiMetaProcessor();
                    break;

                case "Alexa.ConnectedHome.Discover":
                    processor = new AlexaDiscoverProcessor();
                    break;

                case "Alexa.ConnectedHome.Control":
                case "Alexa.ConnectedHome.Query":
                default:
                    break;    
            }
            if (processor == null) throw new Exception($"Namespace {alexaRequest.Header.Namespace} is not supported");

            Log.Info($"Sending request '{alexaRequest.Header.Name}' to processor '{processor.Name}'");
            var response = await processor.ProcessRequest(alexaRequest.Header.Name, alexaRequest.Payload, context);
            Log.Debug("Returning response...");

            return response;
        }
    }
}
