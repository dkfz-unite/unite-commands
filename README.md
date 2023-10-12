# Command web wrapper service

## General
This service allows to run dockerized command scripts using web API.

Service provides the following functionality:
- [Command web API](Docs/api.md) - REST API for running the command.

Command web service is written in ASP.NET (.NET 7)

## Access
Environment|Address|Port
-----------|-------|----
Host|http://localhost:5300|5300

## Configuration
To configure the application, change environment variables in either docker or [launchSettings.json](Unite.Commands.Web/Properties/launchSettings.json) file (if running locally):

**`UNITE_COMMAND`*** - Command to run.
- All entries of `{src}` will be replaced bu the source directory.
- All entries of `{data}` will be replaced bu the data directory.
- All entries of `{proc}` will be replaced bu the process key.
- Example: `"sh"`

**`UNITE_COMMAND_ARGUMENTS`** - Command arguments.
- All entries of `{src}` will be replaced bu the source directory.
- All entries of `{data}` will be replaced bu the data directory.
- All entries of `{proc}` will be replaced bu the process key.
- Example: `"script.sh -i {data}/{proc}_input.txt -o {data}/{proc}_output.txt"`

**`UNITE_SOURCE_PATH`** - Source directory.
- Notes: Command always starts in the source directory. If the source directory is not specified, the command starts in the current dirrectory.
- Example: `"/analysis"`

**`UNITE_DATA_PATH`** - Data directory.
- Notes: Data dirrectory is usually a mounted volume of the container.
- Example: `"/mnt/data"`

**`UNITE_LIMIT`** - Maximum amount of the commands to run simultaneously.
- Notes: if not specified, the service will run commands without any limit.
- Limitations: Greater than 0.
- Example: `"5"`

## Installation
Service is published as self-contained single-file executable. Running it does not require any additional dependencies.

### Docker
#### Part of the command
To make the service part of existing command application, the following steps are required:

Clone pre-buit application files for required platform from project [publish](publish) directory to the directory of your command **Dockerfile** (e.g. `linux-x64`).
```
-app
--...
--Dockerfile
--linux-x64
```

Modify command **Dockerfile** configuring the web API service:
```dockerfile
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENV ASPNETCORE_hostBuilder:reloadConfigOnChange=false
ENV UNITE_COMMAND="sh"
ENV UNITE_COMMAND_ARGUMENTS="script.sh -i {data}/{proc}_input.txt -o {data}/{proc}_output.txt"
ENV UNITE_SOURCE_PATH="/analysis"
ENV UNITE_DATA_PATH="/mnt/data"
WORKDIR /app
EXPOSE 80
```

Copy web API service files to the container:
```dockerfile
COPY ./linux-x64 /app 
```

Make the web service a startup command:
```dockerfile
CMD ["/app/Unite.Commands.Web", "--urls", "http://0.0.0.0:80"]
```

#### Standalone
Normaly the service runs as a part of the command application.
To run the service as a standalone application (for testing purposes), the following steps are required:

[Dockerfile](Dockerfile) is used to build an image of the application.
To build an image run the following command:
```bash
docker build -t unite.commands:latest .
```

All application components should run in the same docker network.
To create common docker network if not yet available run the following command:
```bash
docker network create unite
```

To run application in docker run the following command:
```bash
docker run \
--name unite.commands \
--restart unless-stopped \
--net unite \
--net-alias commands.unite.net \
-p 127.0.0.1:5300:80 \
-e ASPNETCORE_ENVIRONMENT=Release \
-e UNITE_COMMAND="ls" \
-e UNITE_COMMAND_ARGUMENTS="-la" \
-d \
unite.commands:latest
```