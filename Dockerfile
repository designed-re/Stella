# Stella server Dockerfile — builds the .NET 10 solution from source and runs
# the Stella web server. Plugins are published separately and placed in the
# host's plugins/ directory; Data/Seed assets are copied alongside.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the project files first to cache NuGet restore.
COPY Stella.slnx ./
COPY Stella.Abstractions/Stella.Abstractions.csproj Stella.Abstractions/
COPY Stella/Stella.csproj Stella/
COPY CorePlugin/CorePlugin.csproj CorePlugin/
COPY StellaKFCPlugin/StellaKFCPlugin.csproj StellaKFCPlugin/
COPY Stella.MigrationHelper/Stella.MigrationHelper.csproj Stella.MigrationHelper/
COPY TestClient/TestClient.csproj TestClient/
COPY KBinXml.Net/src/KbinXml.Net/KbinXml.Net.csproj KBinXml.Net/src/KbinXml.Net/

RUN dotnet restore Stella.slnx

# Copy the rest of the source (including Data/Seed assets).
COPY . .

# Publish the Stella host.
RUN dotnet publish Stella/Stella.csproj -c Release -o /app /p:UseAppHost=false

# Publish each plugin into its own output dir, then assemble /app/plugins/.
RUN dotnet publish CorePlugin/CorePlugin.csproj -c Release -o /plugins/core /p:UseAppHost=false
RUN dotnet publish StellaKFCPlugin/StellaKFCPlugin.csproj -c Release -o /plugins/kfc /p:UseAppHost=false

RUN mkdir -p /app/plugins \
  && cp /plugins/core/CorePlugin.dll /app/plugins/ \
  && cp /plugins/kfc/StellaKFCPlugin.dll /app/plugins/ \
  && cp /src/CorePlugin/plugin_core.example.json /app/plugins/plugin_core.json \
  && cp /src/StellaKFCPlugin/plugin_kfc.example.json /app/plugins/plugin_kfc.json

# Copy the KFC static-data seed assets into /app/Data/Seed/. The asphyxia_data.json
# is small (130KB); music_db.xml is large (8.4MB) and is volume-mounted at
# runtime by docker-compose so users can swap it without rebuilding.
RUN mkdir -p /app/Data/Seed \
  && cp /src/StellaKFCPlugin/Data/Seed/asphyxia_data.json /app/Data/Seed/ \
  && (cp /src/StellaKFCPlugin/Data/Seed/music_db.xml /app/Data/Seed/ 2>/dev/null || true)

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app /app

COPY docker/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV STELLA_KFC_DB="Server=db;Port=3306;User ID=stella;Password=stella;Database=stella_kfc"
ENV STELLA_CORE_DB="Server=db;Port=3306;User ID=stella;Password=stella;Database=stella_core"

ENTRYPOINT ["/entrypoint.sh"]