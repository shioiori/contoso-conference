# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Ticketing/Ticketing.API/Ticketing.API.csproj", "Ticketing/Ticketing.API/"]
COPY ["EventBus/EventBus.Core/EventBus.Core.csproj", "EventBus/EventBus.Core/"]
COPY ["Shared/Eventbox.Shared/Eventbox.Shared.csproj", "Shared/Eventbox.Shared/"]
COPY ["EventBus/EventBus.RabbitMQ/EventBus.RabbitMQ.csproj", "EventBus/EventBus.RabbitMQ/"]
COPY ["Ticketing/Ticketing.Application/Ticketing.Application.csproj", "Ticketing/Ticketing.Application/"]
COPY ["Shared/Eventbox.Contracts/Eventbox.Contracts.csproj", "Shared/Eventbox.Contracts/"]
COPY ["Ticketing/Ticketing.Domain/Ticketing.Domain.csproj", "Ticketing/Ticketing.Domain/"]
COPY ["Ticketing/Ticketing.Infrastructure/Ticketing.Infrastructure.csproj", "Ticketing/Ticketing.Infrastructure/"]
RUN dotnet restore "./Ticketing/Ticketing.API/Ticketing.API.csproj"
COPY . .
WORKDIR "/src/Ticketing/Ticketing.API"
RUN dotnet build "./Ticketing.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ticketing.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ticketing.API.dll"]