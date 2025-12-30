# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["BMPTec.Api/BMPTec.Api.csproj", "BMPTec.Api/"]
COPY ["BMPTec.Application/BMPTec.Application.csproj", "BMPTec.Application/"]
COPY ["BMPTec.Domain/BMPTec.Domain.csproj", "BMPTec.Domain/"]
COPY ["BMPTec.Infrastructure/BMPTec.Infrastructure.csproj", "BMPTec.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "BMPTec.Api/BMPTec.Api.csproj"

# Copy everything
COPY . .

# Build
WORKDIR "/src/BMPTec.Api"
RUN dotnet build "BMPTec.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "BMPTec.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install SQL Server tools for health checks (opcional)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BMPTec.Api.dll"]