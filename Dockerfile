FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ["src/Oficina.Api/Oficina.Api.csproj", "src/Oficina.Api/"]
COPY ["src/Oficina.Application/Oficina.Application.csproj", "src/Oficina.Application/"]
COPY ["src/Oficina.Domain/Oficina.Domain.csproj", "src/Oficina.Domain/"]
COPY ["src/Oficina.Infrastructure/Oficina.Infrastructure.csproj", "src/Oficina.Infrastructure/"]
RUN dotnet restore "src/Oficina.Api/Oficina.Api.csproj"

COPY . .

WORKDIR "/app/src/Oficina.Api"
RUN dotnet publish "Oficina.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Oficina.Api.dll"]
