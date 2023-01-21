#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["KinMai.Api/KinMai.Api.csproj", "KinMai.Api/"]
COPY ["KinMai.Authentication/KinMai.Authentication.csproj", "KinMai.Authentication/"]
COPY ["KinMai.Common/KinMai.Common.csproj", "KinMai.Common/"]
COPY ["KinMai.Dapper/KinMai.Dapper.csproj", "KinMai.Dapper/"]
COPY ["KinMai.EntityFramework/KinMai.EntityFramework.csproj", "KinMai.EntityFramework/"]
COPY ["KinMai.Logic/KinMai.Logic.csproj", "KinMai.Logic/"]
COPY ["KinMai.S3/KinMai.S3.csproj", "KinMai.S3/"]
COPY ["KinMai.UnitTests/KinMai.UnitTests.csproj", "KinMai.UnitTests/"]
RUN dotnet restore "KinMai.Api/KinMai.Api.csproj"
COPY . .
WORKDIR "/src/KinMai.Api"
RUN dotnet build "KinMai.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KinMai.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KinMai.Api.dll"]