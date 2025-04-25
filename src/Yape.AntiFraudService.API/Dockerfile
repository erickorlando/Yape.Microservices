FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Yape.AntiFraudService.API/Yape.AntiFraudService.API.csproj", "src/Yape.AntiFraudService.API/"]
COPY ["src/Yape.AntiFraudService.Application/Yape.AntiFraudService.Application.csproj", "src/Yape.AntiFraudService.Application/"]
COPY ["src/Yape.AntiFraudService.Domain/Yape.AntiFraudService.Domain.csproj", "src/Yape.AntiFraudService.Domain/"]
COPY ["src/Yape.AntiFraudService.Infrastructure/Yape.AntiFraudService.Infrastructure.csproj", "src/Yape.AntiFraudService.Infrastructure/"]
RUN dotnet restore "src/Yape.AntiFraudService.API/Yape.AntiFraudService.API.csproj"
COPY . .
WORKDIR "/src/src/Yape.AntiFraudService.API"
RUN dotnet build "./Yape.AntiFraudService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Yape.AntiFraudService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yape.AntiFraudService.API.dll"]
