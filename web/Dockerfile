# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /ASMPT.web/app

COPY ["web/WebApp.csproj", "web/"]
COPY ["core/core.csproj", "core/"]

RUN dotnet restore "web/WebApp.csproj"

COPY ["web/", "web/"]
COPY ["core/", "core/"]

RUN dotnet publish "web/WebApp.csproj" -c Release -o /ASMPT.web/app/publish

# Runtime stage
FROM nginx:alpine

COPY web/nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build /ASMPT.web/app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80

CMD ["nginx","-g", "daemon off;"]