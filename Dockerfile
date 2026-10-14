FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY *.sln .

COPY ["src/Oficina.Api/Oficina.Api.csproj", "Oficina.Api/"]
COPY ["src/Oficina.Application/Oficina.Application.csproj", "Oficina.Application/"]
COPY ["src/Oficina.Domain/Oficina.Domain.csproj", "Oficina.Domain/"]
COPY ["src/Oficina.Infrastructure/Oficina.Infrastructure.csproj", "Oficina.Infrastructure/"]
RUN dotnet restore "src/Oficina.Api/Oficina.Api.csproj"

COPY . .
WORKDIR /scr/Oficina.Api
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app ./

ENTRYPOINT ["dotnet", "Oficina.Api.dll"]
