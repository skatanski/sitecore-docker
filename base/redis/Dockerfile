# escape=`
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.7.2-windowsservercore-ltsc2019

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Install Redis
ARG REDIS_VERSION=3.0.504

RUN Invoke-WebRequest -Uri ('https://github.com/microsoftarchive/redis/releases/download/win-{0}/Redis-x64-{0}.zip' -f $ENV:REDIS_VERSION) -OutFile /redis.zip; `
    Expand-Archive -Path /redis.zip -DestinationPath c:\redis; `
    Remove-Item /redis.zip -Force

ENTRYPOINT ["powershell", "C:/redis/redis-server.exe"]
