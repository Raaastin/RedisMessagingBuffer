#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#########################
# Argument for build 
#########################
ARG RUNTIME_IMAGE=mcr.microsoft.com/dotnet/aspnet:8.0
ARG BUILD_IMAGE=mcr.microsoft.com/dotnet/sdk:8.0

########################################################
# IMAGES For build and Runtime
# Framework : Net 6.0
# Use Alpine a runtime Image
########################################################
FROM $RUNTIME_IMAGE AS base
WORKDIR /app
EXPOSE 80 
EXPOSE 443

###################################################
# Build on sdk image
###################################################
FROM $BUILD_IMAGE AS build
WORKDIR /app

COPY ./Messaging.Buffer ./Messaging.Buffer
COPY ./WebAppExample ./WebAppExample
COPY ./nuget.config ./nuget.config

#RUN dotnet restore "WebAppExample/WebAppExample.csproj"

RUN dotnet build "Messaging.Buffer/Messaging.Buffer.csproj" -c Release -o /app
RUN dotnet build "WebAppExample/WebAppExample.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebAppExample/WebAppExample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAppExample.dll"]