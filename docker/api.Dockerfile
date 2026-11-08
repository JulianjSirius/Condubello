# Contexto de construcción: raíz del repositorio
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY Condubello.Domain/*.csproj Condubello.Domain/
COPY Condubello.Application/*.csproj Condubello.Application/
COPY Condubello.Infrastructure/*.csproj Condubello.Infrastructure/
COPY Condubello.Api/*.csproj Condubello.Api/
RUN dotnet restore Condubello.Api/Condubello.Api.csproj
COPY Condubello.Domain Condubello.Domain
COPY Condubello.Application Condubello.Application
COPY Condubello.Infrastructure Condubello.Infrastructure
COPY Condubello.Api Condubello.Api
RUN dotnet publish Condubello.Api/Condubello.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Condubello.Api.dll"]
