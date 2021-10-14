FROM mcr.microsoft.com/dotnet/core/sdk:5.0-alpine AS build-env

# Set the working directory to /app
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . ./
RUN dotnet restore

# build
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:5.0
WORKDIR /app
COPY --from=build-env /app/web/out .

ENV ASPNETCORE_URLS http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "web.dll"]
