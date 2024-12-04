#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Replybot/Replybot.csproj", "Replybot/"]
RUN dotnet restore "Replybot/Replybot.csproj"
COPY . .
WORKDIR "/src/Replybot"
RUN dotnet build "Replybot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Replybot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Replybot.dll"]
