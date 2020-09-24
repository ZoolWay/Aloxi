# Aloxi
Alexa Loxone Smart Home Integration, 3rd edition.

After a developer skills and two proxies I started to use the Alexa SmartHome SDK instead of the default Alexa Skill SDK. This way I will no longer have to say the name of my skill and the integration goes deeper and I can even use Amazon Smart Home rules.

# AWS IoT Core

TODO: Cover how to configure AWS IoT Core

- Create an object
    - e.g. named "aloxi-bridge"
    - create a certficate and download it and its keys and if you don't have them the Amazon trusted root CA certs, you'll need them in the Aloxi Bridge to connect the bridge to IoT Core.
- Configure
    - Got to _Iot Core_, _Safe_, _Policies_ and create a policy to describe who might publish, subscribe, etc. to the Bridge. Bad but for starter:
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
  - Under "security" of the object, attach a policy (those are not global IAM policies) and activate the cert if not alread active.


# TODO

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
- Dimable light controller
- Jalousine controller
- SONOS adapter
  - list devices/groups
  - make them available named for virtual output in loxone
  - https://developer.sonos.com/build/connected-home-get-started/features/
- REST commander
  - send echo-request to MQTT
  - send echo-request to lambda
  - send test-push
  - stop bridge (will require to log into raspberry to restart!)
