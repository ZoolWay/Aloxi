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

Check **status**:
`sudo systemctl status aloxi`

**Start** it:
`sudo systemctl start aloxi`

**Stop** it:
`sudo systemctl stop aloxi`

**Enable** with machine startup:
`sudo systemctl enable aloxi.service`

Show **log** with journalctl:
`sudo journalctl -a -u aloxi`

See <https://devblogs.microsoft.com/dotnet/net-core-and-systemd/>.
