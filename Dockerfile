# ETAPA 1: Construcción y Compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiamos los archivos de proyecto para aprovechar la caché de capas de Docker
COPY ["EmpadronamientoBackend.sln", "./"]
COPY ["API/API.csproj", "API/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

# Restauramos los paquetes NuGet de toda la solución
RUN dotnet restore "EmpadronamientoBackend.sln"

# Copiamos el resto del código fuente del proyecto
COPY . .

# Nos posicionamos en el proyecto Web/API y compilamos en modo Release
WORKDIR "/src/API"
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ETAPA 2: Entorno de ejecución final ligero
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Puerto interno en el que escuchará la API (.NET 8/9 usan 8080 por defecto)
EXPOSE 8080

# Copiamos los binarios optimizados desde la etapa de compilación
COPY --from=build /app/publish .

# Comando de arranque del contenedor
ENTRYPOINT ["dotnet", "API.dll"]