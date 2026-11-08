# Contexto de construcción: raíz del repositorio
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY Condubello.Gateway/*.csproj Condubello.Gateway/
RUN dotnet restore Condubello.Gateway/Condubello.Gateway.csproj
COPY Condubello.Gateway Condubello.Gateway
RUN dotnet publish Condubello.Gateway/Condubello.Gateway.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Condubello.Gateway.dll"]
