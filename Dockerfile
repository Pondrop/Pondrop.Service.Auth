#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Pondrop.Service.Auth.Api/Pondrop.Service.Auth.Api.csproj", "src/Pondrop.Service.Auth.Api/"]
COPY ["src/Pondrop.Service.Auth.Application/Pondrop.Service.Auth.Application.csproj", "src/Pondrop.Service.Auth.Application/"]
COPY ["src/Pondrop.Service.Auth.Domain/Pondrop.Service.Auth.Domain.csproj", "src/Pondrop.Service.Auth.Domain/"]
COPY ["src/Pondrop.Service.Auth.Infrastructure/Pondrop.Service.Auth.Infrastructure.csproj", "src/Pondrop.Service.Auth.Infrastructure/"]
RUN dotnet nuget add source "https://pkgs.dev.azure.com/PondropDevOps/_packaging/PondropDevOps/nuget/v3/index.json" --name "PondropInfrastructure" --username "user" --password "qylafrpgp6sxjwuvmipxajoq3yh5qwg3d5encx27mom5bmn3naza" --store-password-in-clear-text
RUN dotnet restore "src/Pondrop.Service.Auth.Api/Pondrop.Service.Auth.Api.csproj"
COPY . .
WORKDIR "/src/src/Pondrop.Service.Auth.Api"
RUN dotnet build "Pondrop.Service.Auth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pondrop.Service.Auth.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pondrop.Service.Auth.Api.dll"]