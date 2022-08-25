# Filename: Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

RUN ls 

COPY ../Tools/* .
RUN Nuke/build.cmd InstallDocusaurus