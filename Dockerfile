# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy và restore
COPY WebAppMR/WebAppMR.csproj WebAppMR/
RUN dotnet restore WebAppMR/WebAppMR.csproj

# Copy toàn bộ code và publish
COPY . .
WORKDIR /src/WebAppMR
RUN dotnet publish -c Release -o /app --no-restore

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy từ build stage
COPY --from=build /app ./

# Quan trọng: bind port do Render cung cấp
# Kestrel sẽ nghe trên tất cả interfaces (http://0.0.0.0) và cổng $PORT
ENV ASPNETCORE_URLS=http://+:$PORT

# Khi cần, bạn có thể EXPOSE 80, nhưng không bắt buộc cho Render
# EXPOSE 80

ENTRYPOINT ["dotnet", "WebAppMR.dll"]
