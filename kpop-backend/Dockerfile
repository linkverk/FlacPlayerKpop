FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["KPopFlacMusicServer.csproj", "./"]
RUN dotnet restore "KPopFlacMusicServer.csproj"
COPY . .
RUN dotnet build "KPopFlacMusicServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KPopFlacMusicServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create music directory
RUN mkdir -p /app/music

# Run as non-root user
RUN useradd -m appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "KPopFlacMusicServer.dll"]
