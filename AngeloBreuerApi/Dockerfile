#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AngeloBreuerApi/AngeloBreuerApi.csproj", "AngeloBreuerApi/"]
RUN dotnet nuget add source https://nuget.angelobreuer.de/v3/index.json
RUN dotnet restore "AngeloBreuerApi/AngeloBreuerApi.csproj"
COPY . .
WORKDIR "/src/AngeloBreuerApi"
RUN dotnet build "AngeloBreuerApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AngeloBreuerApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AngeloBreuerApi.dll"]