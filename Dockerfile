# Stella server Dockerfile — builds the .NET 10 solution from source and runs
# the Stella web server (e-amusement endpoints + the /webui admin UI on the
# same port). Plugins are published separately and placed in /app/plugins/;
# Data/Seed assets are copied alongside. Configuration is JSON-only and is
# mounted over the app by docker-compose (see docker/*.json).

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

# Copy the rest of the source (including Data/Seed assets and committed
# Tailwind CSS under Stella/wwwroot — no npm build is needed in the image).
COPY . .

# Publish the Stella host.
RUN dotnet publish Stella/Stella.csproj -c Release -o /app /p:UseAppHost=false

# Publish each plugin into its own output dir, then assemble /app/plugins/.
RUN dotnet publish CorePlugin/CorePlugin.csproj -c Release -o /plugins/core /p:UseAppHost=false
RUN dotnet publish StellaKFCPlugin/StellaKFCPlugin.csproj -c Release -o /plugins/kfc /p:UseAppHost=false

RUN mkdir -p /app/plugins \
  && cp /plugins/core/CorePlugin.dll /app/plugins/ \
  && cp /plugins/kfc/StellaKFCPlugin.dll /app/plugins/ \
  && cp /src/CorePlugin/plugin_core.json /app/plugins/plugin_core.json \
  && cp /src/StellaKFCPlugin/plugin_kfc.json /app/plugins/plugin_kfc.json

# Copy the KFC static-data seed assets into /app/Data/Seed/. asphyxia_data.json
# (~130KB) is required by the WebUI "Seed static data" action. music_db.xml
# (~8.4MB) is NOT committed; if present at build time it is baked in, otherwise
# it is uploaded at runtime via the WebUI "Data" page. docker-compose mounts a
# named volume over /app/Data/Seed so uploads persist and asphyxia_data.json is
# preserved (Docker copies image contents into the volume on first boot).
RUN mkdir -p /app/Data/Seed \
  && cp /src/StellaKFCPlugin/Data/Seed/asphyxia_data.json /app/Data/Seed/ \
  && cp /src/StellaKFCPlugin/Data/Seed/events_list.json /app/Data/Seed/ \
  && (cp /src/StellaKFCPlugin/Data/Seed/music_db.xml /app/Data/Seed/ 2>/dev/null || true)

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app /app

COPY docker/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# e-amusement POST routes (/eamuse, /core) and the WebUI (/webui) share this port.
EXPOSE 8080

ENTRYPOINT ["/entrypoint.sh"]
