
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

# ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /project

COPY ["./src/Integrations/ToERP/ToErp.APP/ToErp.APP.csproj", "./src/Integrations/ToERP/ToErp.APP/"]

COPY ["./src/Integrations/ToERP/Odoo/Odoo.csproj", "./src/Integrations/ToERP/Odoo/"]

COPY ["./src/BuildingBlocks/EventBus/EventBus.Base/EventBus.Base.csproj", "./src/BuildingBlocks/EventBus/EventBus.Base/"]
COPY ["./src/BuildingBlocks/EventBus/EventBus.Factory/EventBus.Factory.csproj", "./src/BuildingBlocks/EventBus/EventBus.Factory/"]
COPY ["./src/BuildingBlocks/EventBus/EventBus.RabbitMQ/EventBus.RabbitMQ.csproj", "./src/BuildingBlocks/EventBus/EventBus.RabbitMQ/"]
COPY ["./src/BuildingBlocks/EventBus/EventBus.Service/EventBus.Service.csproj", "./src/BuildingBlocks/EventBus/EventBus.Service/"]


RUN dotnet restore "./src/Integrations/ToERP/ToErp.APP/ToErp.APP.csproj"
COPY . .
WORKDIR "/project/src/Integrations/ToERP/ToErp.APP/"

RUN dotnet build "ToErp.APP.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ToErp.APP.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToErp.APP.dll"]