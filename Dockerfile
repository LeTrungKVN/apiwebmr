# Use the official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY WebAppMR/WebAppMR.csproj WebAppMR/
RUN dotnet restore WebAppMR/WebAppMR.csproj

# Copy the rest of the source code
COPY . .
WORKDIR /src/WebAppMR

# Publish the application to the /app folder
RUN dotnet publish -c Release -o /app --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Start the application
ENTRYPOINT ["dotnet", "WebAppMR.dll"]
