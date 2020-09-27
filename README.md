# Aloxi
Alexa Loxone Smart Home Integration, 3rd edition.

After a developer skills and two proxies I started to use the Alexa SmartHome SDK instead of the default Alexa Skill SDK. 
This way I will no longer have to say the name of my skill and the integration goes deeper and I can even use Amazon Smart Home rules.

## AWS IoT Core

An IoT Core object is created and the Aloxi.Bridge will use it to communicate with MQTT-pub/sub (while the lambda function uses policies to access MQTT-pub/sub).

The object will have a certificate with a local policy. That will be downloaded and used by the Aloxi.Bridge.

- Create an object
  - e.g. named "aloxi-bridge"
  - Type/Group is optional
- Create a certficate and download it and its keys and if you don't have them the Amazon trusted root CA certs, you'll need them in the Aloxi Bridge to connect the bridge to IoT Core.
  - You need to combine certificate (`.pem.crt`) and private key (`*-private.pem.key`) into a `.pfx`. This
    can be done with OpenSSL, see `gen_pfx_from_key_and_cert.sh` (Linux or WSL).
- Configure
    - Got to _Iot Core_, _Safe_, _Policies_ and create a policy to describe who might publish, subscribe, etc. to the Bridge. Bad (because resource *) but for starter:
        ```
        {
          "Version": "2012-10-17",
          "Statement": [
            {
              "Effect": "Allow",
              "Action": [
                "iot:Publish",
                "iot:Subscribe",
                "iot:Connect",
                "iot:Receive"
              ],
              "Resource": [
                "*"
              ]
            }
          ]
        }
        ```
  - Under "security" of the object, attach a policy (those are not global IAM policies) and activate the cert if not activated before.

## AWS Lambda (Aloxi.AlexaAdapter)

### Role for the Lambda

Create a role for the lambda. Attach _AWSIotLogging_, _AWSIoTDataAccess_, _AWSLambdaBasicExecutionRole_.

### Create Lambda

The lambda function will be called from the Alexa Skill and publish to IoT Core MQTT and subscribe for answers there.

Deploy the lambda function (from IDE). Select the role created just before. Note its ARN as you will need it later when setting up the Alexa Skill.

On the web console, set up the environment variables to configure the lambda:

* ENDPOINT: Check AWS IoT Core setting for your custom endpoint, looks like _xxxxxxxxxxxxx-ats.iot.region.amazonaws.com_
* CA_PATH: `embed-cert/AmazonRootCA1.pem` evil solution, see below
* CERT_PATH:`embed-cert/xxxxxxxxx-certificate.pfx` evil solution, see below
* CLIENT_ID: Some unique ID, e.g. `aloxi-adapter`, the lambda will prefix generated GUIDs with this as it will internally generate two MQTT clients
* TOPIC_BRIDGE: MQTT topic for the adapter to send messages to the bridge, e.g. `aloxi:to-bridge`
* TOPIC_RESPONSE: MQTT topic for the adapter to subscribe for messages from the bridge, e.g. `aloxi:alexa-response`
* LAMBDA_LOG: Log-Level, for PROD-usage select `WARN`, for testing select `DEBUG`

The MQTT topics must match the configuration on the bridge.

Okay, also we have to embed the device cert into the lambda itself. So it really is hardcoded for
us and it is kind of __evil__. But I could not get the method `ConstructClientDirectlyInAws` in
class `PubSubClient` to work without cert. Copy the root CA cert and the generated/combined pfx
into `embed-cert` and select for _Copy to output directoy_ the value _Copy if newer_ in Visual
Studio. Then redeploy to AWS to upload these files together with the lambda assembly.

## Alexa Smart Home Skill Setup

### Start to create Smart Home skill

* Be sure to select _Smart Home_
* Use Payload version _v3 (preferred)_
* Note the skill id (you'll have to add it later to the lambda trigger)
* As default endpoint enter the ARN of the lambda from above.
* Before you can set up _Account Linking_, set up LWA (or any other OAuth2 provider you want to use - but it is mandatory to have one)

### Setup Login with Amazon (LWA)

An Oauth-login is required for every smart home skill. For this "hard coded" sample we can just use the
LWA service.

* Create LWA-policy
  * Go to <https://developer.amazon.com/dashboard>.
  * Click _Login with Amazon_ (might link to <https://developer.amazon.com/loginwithamazon/console/site/lwa/overview.html>).
  * Create a new security profile.
  * Remember client-id and secret.
* Process with skill setup _Account Linking_
  * Setttings
    * _Allow users to link their account to your skill from within your application or website_: NOT required
    * _Allow users to authenticate using your mobile application_: really NOT required
  * Grant type: _Auth Code Grant__
  * Web Authorization URI: <https://www.amazon.com/ap/oa>
  * Access Token URI: <https://api.amazon.com/auth/o2/token>
  * Client ID: value from security profile
  * Secret: value from security profile
  * Authentication scheme: _HTTP Basic__
  * Scope: Add one, named _profile_
  * Note the redirect URLs for the following step
* Add the redirect-URLs shown at the skill in LWA
  * Go to your LWA security policy
  * Select web settings
  * Add all redirect URLs

### Continue with skill

Actually the skill is mostly done as we do not plan to publish/distribute it, only use it as dev-skill within the
same Alexa account. Otherwise you would need to continue with it to go at least into beta stage and I believe in the
current state there is too much hardcoding

### Add lambda trigger

Go back to your lambda and add a trigger _Alexa Smart Home_. Enter the skill id and activate it.

## Aloxi.Bridge setup

The bridge is designed to work on a Raspberrry Pi 3 as Ready-to-Run executable (.NET core 3.1 is incorporated into the
binary). Also it should be possible to have it running on any platform which supports .NET core 3.1.

Install the binary there and put the device certs (root pem as well as combined pfx) in a subfolder, configure
`appsettings.json` to use the certs.

To automate deployment of updated sources take a look at `deploy-to-smartpi.sh`.

TODO: Incorporate <Aloxi.Bridge/readme.md> here.

## TODO

- Lambda without cert?
- REST Statuscontroller
  - Show home model
  - Publish all errors and warning to a max-buffer and show them
  - Show raspberry OS data
  - Show raspberry disk status
- Error/Warn/Info buffered log
  - Supply logs to REST statuscontroller
  - Special error states should trigger virtual-input of loxone which triggers push-notification
    - errors occured
    - not connected for some time
- SONOS adapter
  - list devices/groups
  - make them available named for virtual output in loxone
  - https://developer.sonos.com/build/connected-home-get-started/features/
  - not sure now as I sum up different smart home devices just directly with Amazon Alexa
- REST commander
  - send echo-request to MQTT
  - send echo-request to lambda
  - send test-push
  - stop bridge (will require to log into raspberry to restart!)
- Alexa
  - make use of async features of payload version 3!


## Notes

I guess Alexa is a trademark of Amazon. No advertising here, just mention a solution with a specific device. 
I am not associated with Amazon in any way.