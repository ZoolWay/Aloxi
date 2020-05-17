using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Akka.Actor;
using Akka.Event;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    public abstract class LoxoneCommBaseActor : ReceiveActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        protected readonly LoxoneConfig loxoneConfig;
        protected readonly JsonSerializerSettings jsonSettings;

        public LoxoneCommBaseActor(LoxoneConfig loxoneConfig)
        {
            this.loxoneConfig = loxoneConfig;
            this.jsonSettings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        protected HttpClient GetLoxoneHttpClient()
        {
            HttpClient http = new HttpClient();
            http.BaseAddress = new Uri($"http://{this.loxoneConfig.Miniserver}");
            var credBytes = Encoding.UTF8.GetBytes($"{loxoneConfig.Username}:{loxoneConfig.Password}");
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credBytes));
            return http;
        }
    }
}
