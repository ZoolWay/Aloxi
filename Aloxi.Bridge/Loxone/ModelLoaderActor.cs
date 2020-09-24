using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Newtonsoft.Json;
using ZoolWay.Aloxi.Bridge.Loxone.Data;
using ZoolWay.Aloxi.Bridge.Models;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    public class ModelLoaderActor : LoxoneCommBaseActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly IActorRef adapter;

        public ModelLoaderActor(LoxoneConfig loxoneConfig, IActorRef adapter) : base(loxoneConfig)
        {
            this.adapter = adapter;
            ReceiveAsync<LoxoneMessage.LoadModel>(ReceivedLoadModel);
        }

        private async Task ReceivedLoadModel(LoxoneMessage.LoadModel message)
        {
            var http = GetLoxoneHttpClient();
            HttpResponseMessage response;
            try
            {
                response = await http.GetAsync("data/LoxAPP3.json");
                this.adapter.Tell(new LoxoneMessage.ReportAvailability(true, DateTime.Now));
            }
            catch (HttpRequestException ex)
            {
                log.Error(ex, "Failed to get data from miniserver");
                this.adapter.Tell(new LoxoneMessage.ReportAvailability(false, DateTime.Now));
                throw;
            }
            log.Debug("Got response from miniserver, HTTP {0}", response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                log.Error("Could not get data from miniserver, HTTP {0}, body: {1}", response.StatusCode, body?.Substring(0, 512));
                return;
            }
            log.Debug("Parsing {0} bytes from JSON to object", body.Length);
            LoxAppModel model = JsonConvert.DeserializeObject<LoxAppModel>(body, jsonSettings);

            List<Control> controls = new List<Control>();
            foreach (var cm in model.Controls)
            {
                if (IsIgnored(cm.Value)) continue;

                try
                {
                    // is normal light switch?
                    if (cm.Value.Type == LoxAppModel.ControlTypeModel.Switch)
                    {
                        Control newControl = new Control(ControlType.LightControl,
                            cm.Value.Name,
                            cm.Key,
                            cm.Value.Name,
                            cm.Value.States.ToImmutableDictionary<string, LoxoneUuid>()
                            );
                        controls.Add(newControl);
                    }
                    else
                    {
                        log.Debug("Ignoring model control {0}: {1} ({2})", cm.Key, cm.Value.Name, cm.Value.Type);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed to parse loxone control '{0}', ignoring: {1}", cm.Key, ex.Message);
                }
            }
            Home newModel = new Home(controls.ToImmutableList());
            Sender.Tell(new LoxoneMessage.PublishModel(newModel, DateTime.Now));
        }

        private bool IsIgnored(LoxAppModel.ControlModel controlModel)
        {
            if (this.loxoneConfig.IgnoreCategories.Contains(controlModel.Cat.ToString())) return true;
            return false;
        }
    }
}
