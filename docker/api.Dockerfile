# Build context: raíz del repositorio
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo
COPY Condubello.slnx ./
COPY src/Condubello.Domain/*.csproj src/Condubello.Domain/
COPY src/Condubello.Application/*.csproj src/Condubello.Application/
COPY src/Condubello.Infrastructure/*.csproj src/Condubello.Infrastructure/
COPY src/Condubello.Api/*.csproj src/Condubello.Api/
RUN dotnet restore src/Condubello.Api/Condubello.Api.csproj
COPY src/Condubello.Domain src/Condubello.Domain
COPY src/Condubello.Application src/Condubello.Application
COPY src/Condubello.Infrastructure src/Condubello.Infrastructure
COPY src/Condubello.Api src/Condubello.Api
RUN dotnet publish src/Condubello.Api/Condubello.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Condubello.Api.dll"]
