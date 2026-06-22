FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Notification/Notification.API/Notification.API.csproj", "Notification/Notification.API/"]
COPY ["EventBus/EventBus.Core/EventBus.Core.csproj", "EventBus/EventBus.Core/"]
COPY ["EventBus/EventBus.RabbitMQ/EventBus.RabbitMQ.csproj", "EventBus/EventBus.RabbitMQ/"]
COPY ["Shared/Eventbox.Contracts/Eventbox.Contracts.csproj", "Shared/Eventbox.Contracts/"]
COPY ["Shared/Eventbox.Shared/Eventbox.Shared.csproj", "Shared/Eventbox.Shared/"]
RUN dotnet restore "./Notification/Notification.API/Notification.API.csproj"
COPY . .
WORKDIR "/src/Notification/Notification.API"
RUN dotnet build "./Notification.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Notification.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]
