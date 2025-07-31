FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
EXPOSE 8080

COPY ["SalesService.sln", "SalesService.sln"]
COPY ["SalesService.API/SalesService.API.csproj", "SalesService.API/"]
COPY ["SalesService.Application/SalesService.Application.csproj", "SalesService.Application/"]
COPY ["SalesService.Domain/SalesService.Domain.csproj", "SalesService.Domain/"]
COPY ["SalesService.Infrastructure/SalesService.Infrastructure.csproj", "SalesService.Infrastructure/"]

RUN dotnet restore "SalesService.sln"

COPY . .

WORKDIR "/src/SalesService.API"
RUN dotnet publish "SalesService.API.csproj" -c Release -o /app/publish





FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /src/SalesService.API/app/publish .


ENTRYPOINT ["dotnet", "SalesService.API.dll"]