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


