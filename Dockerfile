# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj файлы проектов, которые нужны
COPY Entities/EntitiesLibrary.csproj Entities/
COPY PasswordHasher/PasswordHasherLibrary.csproj PasswordHasher/
COPY Server/Server.csproj Server/
COPY TokenSession/TokenServiceLibrary.csproj TokenSession/

# Восстанавливаем зависимости только этих проектов
RUN dotnet restore Server/Server.csproj

# Копируем весь код
COPY . .

# Публикуем Server
WORKDIR /src/Server
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
RUN mkdir -p /app/uploads
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_URLS=http://+:8888
ENTRYPOINT ["dotnet", "Server.dll"]
