FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project files and restore dependencies
COPY CodersGear/CodersGear.csproj ./CodersGear/
COPY CodersGear.DataAccess/CodersGear.DataAccess.csproj ./CodersGear.DataAccess/
COPY CodersGear.Models/CodersGear.Models.csproj ./CodersGear.Models/
COPY CodersGear.Utility/CodersGear.Utility.csproj ./CodersGear.Utility/

# Restore dependencies (preview packages are allowed by default when specified in csproj)
RUN dotnet restore CodersGear/CodersGear.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish CodersGear/CodersGear.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

# Railway sets PORT environment variable
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

EXPOSE 8080

ENTRYPOINT ["dotnet", "CodersGear.dll"]
