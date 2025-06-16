FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Task.PersonDirectory.Api/Task.PersonDirectory.Api.csproj", "src/Task.PersonDirectory.Api/"]
COPY ["src/Task.PersonDirectory.Infrastructure/Task.PersonDirectory.Infrastructure.csproj", "src/Task.PersonDirectory.Infrastructure/"]
COPY ["src/Task.PersonDirectory.Domain/Task.PersonDirectory.Domain.csproj", "src/Task.PersonDirectory.Domain/"]
COPY ["src/Task.PersonDirectory.Application/Task.PersonDirectory.Application.csproj", "src/Task.PersonDirectory.Application/"]
RUN dotnet restore "src/Task.PersonDirectory.Api/Task.PersonDirectory.Api.csproj"
COPY . .
WORKDIR "/src/src/Task.PersonDirectory.Api"
RUN dotnet build "./Task.PersonDirectory.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Task.PersonDirectory.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Task.PersonDirectory.Api.dll"]
