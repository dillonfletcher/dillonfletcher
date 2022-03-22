FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080 2222
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DillonFletcher.csproj", "./"]
RUN dotnet restore "DillonFletcher.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "DillonFletcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DillonFletcher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "DillonFletcher.dll"]