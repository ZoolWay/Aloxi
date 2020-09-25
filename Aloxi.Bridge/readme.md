# Aloxi.Bridge

Runs in your local network. Connects to AWS IoT Core and uses topic subscriptions to receive
commands from Alexa (via Aloxi.AlexaAdapter lambda function).

## API Endpoints

todo

## SystemD configuration

Service file: `/etc/systemd/system/aloxi.service`

```
[Unit]
Description=Aloxi.Bridge application

[Service]
Type=notify
ExecStart=/aloxi/Aloxi.Bridge

[Install]
WantedBy=multi-user.target
```

Load it with `sudo systemctl daemon-reload`

Check status:
`sudo systemctl status aloxi`

Start:
`sudo systemctl start aloxi.service`

Enable with machine startup:
`sudo systemctl enable aloxi.service`

Jounralctl
`sudo journalctl -u aloxi`

See <https://devblogs.microsoft.com/dotnet/net-core-and-systemd/>.
