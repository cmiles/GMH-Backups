#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["GmhWorkshop.Web/GmhWorkshop.Web.csproj", "GmhWorkshop.Web/"]
COPY ["GmhWorkshop.Jobs/GmhWorkshop.Jobs.csproj", "GmhWorkshop.Jobs/"]
COPY ["GmhWorkshop.CommonTools/GmhWorkshop.CommonTools.csproj", "GmhWorkshop.CommonTools/"]
COPY ["GmhWorkshop.SensorPush/GmhWorkshop.SensorPush.csproj", "GmhWorkshop.SensorPush/"]
COPY ["GmhWorkshop.WeatherFlowTempest/GmhWorkshop.WeatherFlowTempest.csproj", "GmhWorkshop.WeatherFlowTempest/"]
RUN dotnet restore "GmhWorkshop.Web/GmhWorkshop.Web.csproj"
COPY . .
WORKDIR "/src/GmhWorkshop.Web"
RUN dotnet build "GmhWorkshop.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GmhWorkshop.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GmhWorkshop.Web.dll"]