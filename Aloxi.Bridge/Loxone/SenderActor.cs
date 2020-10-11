using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

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
            ReceiveAsync<LoxoneMessage.ControlDimmer>(ReceivedControlDimmer);
            ReceiveAsync<LoxoneMessage.ControlBlinds>(ReceivedControlBlinds);
            ReceiveAsync<LoxoneMessage.TestAvailability>(ReceivedTestAvailability);
        }

        private async Task ReceivedTestAvailability(LoxoneMessage.TestAvailability message)
        {
            log.Debug($"Checking miniserver availability");
            var response = await RunLoxoneAsync("dev/sps/status");
            if (response == null) return;
            log.Debug("Miniserver is available");
        }

        private async Task ReceivedControlDimmer(LoxoneMessage.ControlDimmer message)
        {
            log.Debug($"Dim '{message.LoxoneUuid}' {message.Type} to {message.Value}");
            if (message.Type != LoxoneMessage.ControlDimmer.DimType.Set) throw new NotSupportedException($"DimType {message.Type} not supported (yet)!");
            if ((message.Value < 0) || (message.Value > 100)) throw new ArgumentOutOfRangeException("Value", message.Value, "Valid percentage required");
            var response = await RunLoxoneAsync($"dev/sps/io/{HttpUtility.UrlEncode(message.LoxoneUuid)}/{message.Value}%");
        }

        private async Task ReceivedControlSwitch(LoxoneMessage.ControlSwitch message)
        {
            log.Debug($"Switch '{message.LoxoneUuid}' to '{message.DesiredState}'");
            string loxOp = TranslateToLoxoneOperation(message.DesiredState, message.LoxoneUuid);
            var response = await RunLoxoneAsync($"dev/sps/io/{HttpUtility.UrlEncode(message.LoxoneUuid.ToString())}/{loxOp}");
        }

        private async Task ReceivedControlBlinds(LoxoneMessage.ControlBlinds message)
        {
            log.Debug($"Blinds '{message.LoxoneUuid}' set to '{message.Command}'");
            string loxOp = TranslateToLoxoneOperation(message.Command);
            var response = await RunLoxoneAsync($"dev/sps/io/{HttpUtility.UrlEncode(message.LoxoneUuid.ToString())}/{loxOp}");
        }

        private async Task<HttpResponseMessage> RunLoxoneAsync(string requestUri)
        {
            HttpResponseMessage response = null;
            var client = GetLoxoneHttpClient();
            try
            {
                response = await client.GetAsync(requestUri);
                this.adapter.Tell(new LoxoneMessage.ReportAvailability(true, DateTime.Now));
                if (!response.IsSuccessStatusCode)
                {
                    string body = await response.Content.ReadAsStringAsync();
                    log.Error($"Loxone reported non-success HTTP code {response.StatusCode}!\nRequest: {requestUri}\nResponse: {body}");
                }
            }
            catch (HttpRequestException ex)
            {
                log.Error(ex, "Failed to reach Loxone Miniserver");
                this.adapter.Tell(new LoxoneMessage.ReportAvailability(false, DateTime.Now));
            }
            return response;
        }

        private string TranslateToLoxoneOperation(LoxoneMessage.ControlBlinds.BlindCmd command)
        {
            switch (command)
            {
                case LoxoneMessage.ControlBlinds.BlindCmd.FullDown:
                    return "fulldown";
                case LoxoneMessage.ControlBlinds.BlindCmd.FullUp:
                    return "fullup";
                case LoxoneMessage.ControlBlinds.BlindCmd.Stop:
                    return "stop";
            }
            return String.Empty;
        }

        private string TranslateToLoxoneOperation(LoxoneMessage.ControlSwitch.DesiredStateType desiredState, LoxoneUuid uuid)
        {
            switch (desiredState)
            {
                case LoxoneMessage.ControlSwitch.DesiredStateType.On:
                    return "On";
                case LoxoneMessage.ControlSwitch.DesiredStateType.Off:
                    return "Off";
                case LoxoneMessage.ControlSwitch.DesiredStateType.ByUuid:
                    return uuid.ToString();
            }
            string errMsg = $"DesiredStatte '{desiredState}' could not be translated";
            throw new Exception(errMsg);
        }
    }
}
