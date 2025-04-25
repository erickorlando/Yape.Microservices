FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Yape.TransactionService.API/Yape.TransactionService.API.csproj", "src/Yape.TransactionService.API/"]
COPY ["src/Yape.TransactionService.Application/Yape.TransactionService.Application.csproj", "src/Yape.TransactionService.Application/"]
COPY ["src/Yape.TransactionService.Domain/Yape.TransactionService.Domain.csproj", "src/Yape.TransactionService.Domain/"]
COPY ["src/Yape.TransactionService.Infrastructure/Yape.TransactionService.Infrastructure.csproj", "src/Yape.TransactionService.Infrastructure/"]
RUN dotnet restore "src/Yape.TransactionService.API/Yape.TransactionService.API.csproj"
COPY . .
WORKDIR "/src/src/Yape.TransactionService.API"
RUN dotnet build "./Yape.TransactionService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Yape.TransactionService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yape.TransactionService.API.dll"]
