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

# Bind to Render-provided PORT
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# copy published output
COPY --from=build /app/publish .

# optional: health port exposure (Render detects from logs, EXPOSE not required)
# EXPOSE8080

ENTRYPOINT ["dotnet", "AtlasViewer.dll"]