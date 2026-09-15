FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BionicPro.Auth/BionicPro.Auth.csproj", "BionicPro.Auth/"]
RUN dotnet restore "./BionicPro.Auth/BionicPro.Auth.csproj"
COPY . .
WORKDIR "/src/BionicPro.Auth"
RUN dotnet build "./BionicPro.Auth.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BionicPro.Auth.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BionicPro.Auth.dll"]