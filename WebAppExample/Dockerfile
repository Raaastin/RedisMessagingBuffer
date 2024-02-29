FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80 
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./Messaging.Buffer ./Messaging.Buffer
COPY ./WebAppExample ./WebAppExample
COPY ./nuget.config ./nuget.config

RUN dotnet build "WebAppExample/WebAppExample.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "WebAppExample/WebAppExample.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebAppExample.dll"]