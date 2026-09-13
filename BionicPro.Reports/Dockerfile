FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BionicPro.Reports/BionicPro.Reports.csproj", "BionicPro.Reports/"]
RUN dotnet restore "./BionicPro.Reports/BionicPro.Reports.csproj"
COPY . .
WORKDIR "/src/BionicPro.Reports"
RUN dotnet build "./BionicPro.Reports.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BionicPro.Reports.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BionicPro.Reports.dll"]