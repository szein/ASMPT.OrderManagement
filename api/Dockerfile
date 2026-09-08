# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /ASMPT.OrderManagement

COPY ["api/api.csproj", "api/"]
COPY ["core/core.csproj", "core/"]

RUN dotnet restore "api/api.csproj"

COPY ["api/", "api/"]
COPY ["core/", "core/"]

RUN dotnet publish "api/api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /src/api
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "api.dll"]