FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["app/RquestBridge/RquestBridge.csproj", "app/RquestBridge/"]
COPY ["lib/FiveSafes.Net/FiveSafes.Net/FiveSafes.Net.csproj", "lib/FiveSafes.Net/FiveSafes.Net/"]
RUN dotnet restore "app/RquestBridge/RquestBridge.csproj"
COPY . .
WORKDIR "/src/app/RquestBridge"
RUN dotnet build "RquestBridge.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RquestBridge.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RquestBridge.dll"]
