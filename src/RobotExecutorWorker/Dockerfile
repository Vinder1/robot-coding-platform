FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["RobotExecutorWorker/RobotExecutorWorker.csproj", "RobotExecutorWorker/"]
COPY ["RobotCodingPlatform.Core/RobotCodingPlatform.Core.csproj", "RobotCodingPlatform.Core/"]
RUN dotnet restore "RobotExecutorWorker/RobotExecutorWorker.csproj"

COPY . .
WORKDIR "/src/RobotExecutorWorker"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RobotExecutorWorker.dll"]