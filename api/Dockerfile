# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /ASMPT.api/app

COPY ["api/api.csproj", "api/"]
COPY ["core/core.csproj", "core/"]

RUN dotnet restore "api/api.csproj"

COPY ["api/", "api/"]
COPY ["core/", "core/"]

RUN dotnet publish "api/api.csproj" -c Release -o /ASMPT.api/app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /ASMPT.stage/api
RUN mkdir -p /ASMPT.stage/api/data

EXPOSE 8080

COPY --from=build /ASMPT.api/app/publish .

ENTRYPOINT ["dotnet", "api.dll"]