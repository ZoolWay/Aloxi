using System;
using System.Net.Http;
using System.Threading.Tasks;

using Akka.Actor;
using Akka.Event;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    public class SenderActor : LoxoneCommBaseActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);
        private readonly Akka.Actor.IActorRef adapter;

        public SenderActor(LoxoneConfig loxoneConfig, Akka.Actor.IActorRef adapter) : base(loxoneConfig)
        {
            this.adapter = adapter;
            ReceiveAsync<LoxoneMessage.ControlSwitch>(ReceivedControlSwitch);
            ReceiveAsync<LoxoneMessage.TestAvailability>(ReceivedTestAvailability);
        }

        private async Task ReceivedTestAvailability(LoxoneMessage.TestAvailability message)
        {
            log.Debug($"Checking miniserver availability");
            var client = GetLoxoneHttpClient();
            try
            {
                var response = await client.GetAsync("dev/sps/status");
                log.Debug("Miniserver is available");
                this.adapter.Tell(new LoxoneMessage.ReportAvailability(true, DateTime.Now));
            }
            catch (HttpRequestException ex)
            {
                log.Error(ex, "Availability failed");
                this.adapter.Tell(new LoxoneMessage.ReportAvailability(false, DateTime.Now));
            }
        }

        private async Task ReceivedControlSwitch(LoxoneMessage.ControlSwitch message)
        {
            log.Debug($"Switch '{message.LoxoneUuid}' to '{message.DesiredState}'");
            string loxOp = TranslateToLoxoneOperation(message.DesiredState, message.LoxoneUuid);
            var client = GetLoxoneHttpClient();
            try
            {
                var response = await client.GetAsync($"dev/sps/io/{message.LoxoneUuid}/{loxOp}");
                this.adapter.Tell(new LoxoneMessage.ReportAvailability(true, DateTime.Now));
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    string errMsg = $"Loxone reported non-success HTTP code {response.StatusCode}";
                    log.Error(errMsg);
                }
            }
            catch (HttpRequestException ex)
            {
                log.Error(ex, "Failed to reach Loxone");
                this.adapter.Tell(new LoxoneMessage.ReportAvailability(false, DateTime.Now));
            }
        }

        private string TranslateToLoxoneOperation(LoxoneMessage.ControlSwitch.DesiredStateType desiredState, string uuid)
        {
            switch (desiredState)
            {
                case LoxoneMessage.ControlSwitch.DesiredStateType.On:
                    return "On";
                case LoxoneMessage.ControlSwitch.DesiredStateType.Off:
                    return "Off";
                case LoxoneMessage.ControlSwitch.DesiredStateType.ByUuid:
                    return uuid;
            }
            string errMsg = $"DesiredStatte '{desiredState}' could not be translated";
            throw new Exception(errMsg);
        }
    }
}
