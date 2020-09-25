using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
                if (IsIgnored(cm.Key, cm.Value)) continue;
                try
                {
                    string roomName = GetRoomName(cm.Value.Room, model.Rooms);
                    controls.AddRange(ParseControl(cm.Key, cm.Value, roomName));
                }
                catch (Exception ex)
                {
                    log.Error(ex, "Failed to parse loxone control '{0}', ignoring: {1}", cm.Key, ex.Message);
                }
            }
            Home newModel = new Home(controls.ToImmutableList());
            Sender.Tell(new LoxoneMessage.PublishModel(newModel, DateTime.Now));
        }

        private string GetRoomName(LoxoneUuid room, Dictionary<LoxoneUuid, LoxAppModel.RoomModel> rooms)
        {
            if (rooms.ContainsKey(room))
            {
                return rooms[room].Name;
            }
            return "unbekannt";
        }

        private IEnumerable<Control> ParseControl(LoxoneUuid key, LoxAppModel.ControlModel loxControl, string roomName)
        {
            if (loxControl.Type == LoxAppModel.ControlTypeModel.Switch)
            {
                // is normal light switch?
                Control newControl = new Control(ControlType.LightControl,
                    loxControl.Name,
                    key,
                    loxControl.Name,
                    loxControl.States.ToImmutableDictionary<string, LoxoneUuid>(),
                    roomName
                    );
                return new[] { newControl };
            }
            else if (loxControl.Type == LoxAppModel.ControlTypeModel.Dimmer)
            {
                // is dimmer?
                Control newControl = new Control(ControlType.LightDimmableControl,
                    loxControl.Name,
                    key,
                    loxControl.Name,
                    loxControl.States.ToImmutableDictionary<string, LoxoneUuid>(),
                    roomName
                    );
                return new[] { newControl };
            }
            else if (loxControl.Type == LoxAppModel.ControlTypeModel.LightController)
            {
                // defer to subcontrols which should be switches and dimmers
                List<Control> controls = new List<Control>();
                foreach(var sc in loxControl.SubControls)
                {
                    controls.AddRange(ParseControl(sc.Key, sc.Value, roomName));
                }
                return controls;
            }
            log.Debug("Ignoring model control {0}: {1}, not supported type '{2}'", key, loxControl.Name, loxControl.Type);
            return Enumerable.Empty<Control>();
        }

        private bool IsIgnored(LoxoneUuid uuid, LoxAppModel.ControlModel controlModel)
        {
            if (this.loxoneConfig.IgnoreControls.Contains(uuid.ToString())) return true;
            if (this.loxoneConfig.IgnoreCategories.Contains(controlModel.Cat.ToString())) return true;
            return false;
        }
    }
}
