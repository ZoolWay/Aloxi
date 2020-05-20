using System;
using System.Threading.Tasks;
using Akka.Event;

namespace ZoolWay.Aloxi.Bridge.Loxone
{
    public class SenderActor : LoxoneCommBaseActor
    {
        private readonly ILoggingAdapter log = Logging.GetLogger(Context);

        public SenderActor(LoxoneConfig loxoneConfig) : base(loxoneConfig)
        {
            ReceiveAsync<LoxoneMessage.ControlSwitch>(ReceivedControlSwitch);
        }

        private async Task ReceivedControlSwitch(LoxoneMessage.ControlSwitch message)
        {
            log.Debug($"Switch '{message.LoxoneUuid}' to '{message.DesiredState}'");
            string loxOp = TranslateToLoxoneOperation(message.DesiredState, message.LoxoneUuid);
            var client = GetLoxoneHttpClient();
            var response = await client.GetAsync($"dev/sps/io/{message.LoxoneUuid}/{loxOp}");
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                string errMsg = $"Loxone reported non-success HTTP code {response.StatusCode}";
                log.Error(errMsg);
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
