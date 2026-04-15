FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Sery.sln", "./"]
COPY ["src/Sery.Domain/Sery.Domain.csproj", "src/Sery.Domain/"]
COPY ["src/Sery.Application/Sery.Application.csproj", "src/Sery.Application/"]
COPY ["src/Sery.Infrastructure/Sery.Infrastructure.csproj", "src/Sery.Infrastructure/"]
COPY ["src/Sery.API/Sery.API.csproj", "src/Sery.API/"]
COPY ["tests/Sery.Domain.Tests/Sery.Domain.Tests.csproj", "tests/Sery.Domain.Tests/"]
COPY ["tests/Sery.Application.Tests/Sery.Application.Tests.csproj", "tests/Sery.Application.Tests/"]
COPY ["tests/Sery.API.IntegrationTests/Sery.API.IntegrationTests.csproj", "tests/Sery.API.IntegrationTests/"]

RUN dotnet restore "Sery.sln"

COPY . .
RUN dotnet publish "src/Sery.API/Sery.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Sery.API.dll"]
