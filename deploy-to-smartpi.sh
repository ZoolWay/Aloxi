#!/bin/bash

# Use ssh-key for easy authorize

echo -e "\033[1;37mDeploying Aloxi.Bridge to the smartpi\033[0m"

SMARTPI_IP="192.168.0.116"
SMARTPI_USER="pi"
PLATFORM="linux-arm"
CONFIGURATION="Release"

CURR_DIR=`pwd`
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
# echo $CURR_DIR
# echo $DIR

echo -e "\033[1;37m\u21bb Cleaning\033[0m"
cd $DIR/Aloxi.Bridge
rm bin/Release -r

echo -e "\033[1;37m\u21bb Publishing\033[0m"
dotnet publish -r $PLATFORM -c $CONFIGURATION /p:PublishSingleFile=true /p:PublishTrimmed=false

echo -e "\033[1;37m\u21bb Deploying\033[0m"
cd $DIR/Aloxi.Bridge/bin/$CONFIGURATION/netcoreapp3.1/$PLATFORM/publish
ssh $SMARTPI_USER@$SMARTPI_IP 'sudo systemctl stop aloxi.service'
scp Aloxi.Bridge* $SMARTPI_USER@$SMARTPI_IP:/aloxi
ssh $SMARTPI_USER@$SMARTPI_IP 'sudo systemctl start aloxi.service'

echo -e "\033[1;37mCompleted \033[0;32m\u2713\033[0m"
cd $CURR_DIR
