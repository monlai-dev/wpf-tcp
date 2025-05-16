# Build stage using .NET 9 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and publish
COPY . ./
RUN dotnet publish -c Release -o /out

# Runtime stage using .NET 9 runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0-preview
WORKDIR /app
COPY --from=build /out .

EXPOSE 5000
ENTRYPOINT ["dotnet", "ChatTcpServer.dll"]
