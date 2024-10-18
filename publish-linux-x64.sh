#!/bin/bash
os=linux-x64

rm -rf ./publish/$os

# -p:PublishTrimmed=true
dotnet publish Unite.Commands.Web -c Release -r $os -o ./publish/$os -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugType=None --self-contained

mv ./publish/$os/Unite.Commands.Web ./publish/$os/commands
