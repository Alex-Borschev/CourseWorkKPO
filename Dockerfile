# ====== Build stage ======
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY KursuchServ.sln ./
COPY KursuchServ/KursuchServ.csproj KursuchServ/

RUN dotnet restore KursuchServ.sln

COPY KursuchServ/ ./KursuchServ/

RUN dotnet publish KursuchServ/KursuchServ.csproj -c Release -o /app/publish


# ====== Runtime stage ======
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/uploads

EXPOSE 8000

ENTRYPOINT ["dotnet", "KursuchServ.dll"]
