# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY AtlasViewer.csproj ./
RUN dotnet restore "AtlasViewer.csproj"

# copy everything and publish
COPY . ./
RUN dotnet publish "AtlasViewer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Bind to Render-provided PORT and ensure UTF-8 locale
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT} \
 DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Install locales and set UTF-8 default (Debian-based ASP.NET image)
RUN apt-get update && apt-get install -y locales && \
 sed -i '/es_ES.UTF-8/s/^# //g' /etc/locale.gen && \
 sed -i '/en_US.UTF-8/s/^# //g' /etc/locale.gen && \
 locale-gen es_ES.UTF-8 en_US.UTF-8 && \
 rm -rf /var/lib/apt/lists/*
ENV LANG=es_ES.UTF-8 \
 LC_ALL=es_ES.UTF-8 \
 LANGUAGE=es_ES:en \
 LC_CTYPE=es_ES.UTF-8 \
 DOTNET_CLI_TELEMETRY_OPTOUT=1

# copy published output
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AtlasViewer.dll"]