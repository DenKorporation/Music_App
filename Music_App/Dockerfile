FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Music_App/Music_App.csproj", "Music_App/"]
RUN dotnet restore "Music_App/Music_App.csproj"
COPY . .
WORKDIR "/src/Music_App"
RUN dotnet build "Music_App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Music_App.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Music_App.dll"]
