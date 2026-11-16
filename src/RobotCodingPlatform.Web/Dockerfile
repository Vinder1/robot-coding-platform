FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RobotCodingPlatform.Web/RobotCodingPlatform.Web.csproj", "RobotCodingPlatform.Web/"]
COPY ["RobotCodingPlatform.Core/RobotCodingPlatform.Core.csproj", "RobotCodingPlatform.Core/"]

RUN dotnet restore "RobotCodingPlatform.Web/RobotCodingPlatform.Web.csproj"

COPY . .

WORKDIR "/src/RobotCodingPlatform.Web"
RUN dotnet publish "RobotCodingPlatform.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "RobotCodingPlatform.Web.dll"]
